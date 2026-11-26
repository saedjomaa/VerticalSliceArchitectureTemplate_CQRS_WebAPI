using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Results;
using WebAPI.Database;
using WebAPI.Features.Courses;
using WebAPI.Features.Students;

namespace WebAPI.Features.Enrollments.EnrollStudent;

internal sealed class EnrollStudentCommandHandler(SchoolDbContext db, TimeProvider clock)
    : ICommandHandler<EnrollStudentCommand, Result>
{
    public async Task<Result> HandleAsync(EnrollStudentCommand command, CancellationToken cancellationToken)
    {
        if (!await db.Students.AnyAsync(student => student.Id == command.StudentId, cancellationToken))
        {
            return Result.Failure(StudentErrors.NotFound);
        }

        var course = await db.Courses
            .SingleOrDefaultAsync(course => course.Id == command.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        if (await db.Enrollments.AnyAsync(enrollment =>
                enrollment.StudentId == command.StudentId && enrollment.CourseId == command.CourseId, cancellationToken))
        {
            return Result.Failure(EnrollmentErrors.AlreadyEnrolled);
        }

        var enrollment = course.TryEnroll(command.StudentId, clock.GetUtcNow());
        if (enrollment is null)
        {
            return Result.Failure(EnrollmentErrors.CourseFull);
        }

        db.Enrollments.Add(enrollment);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            // The course (EnrolledCount / RowVersion) changed since it was loaded.
            return Result.Failure(EnrollmentErrors.ConcurrencyConflict);
        }
        catch (DbUpdateException exception) when (exception.IsPrimaryKeyViolation(enrollment))
        {
            // Two identical requests passed the AnyAsync check at the same time; the primary key stopped the second.
            return Result.Failure(EnrollmentErrors.AlreadyEnrolled);
        }
    }
}
