using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Results;
using WebAPI.Database;

namespace WebAPI.Features.Enrollments.CancelEnrollment;

internal sealed class CancelEnrollmentCommandHandler(SchoolDbContext db)
    : ICommandHandler<CancelEnrollmentCommand, Result>
{
    public async Task<Result> HandleAsync(CancelEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var registration = await (
            from enrollment in db.Enrollments
            join course in db.Courses on enrollment.CourseId equals course.Id
            where enrollment.StudentId == command.StudentId && enrollment.CourseId == command.CourseId
            select new { Enrollment = enrollment, Course = course }
        ).SingleOrDefaultAsync(cancellationToken);
        if (registration is null)
        {
            return Result.Failure(EnrollmentErrors.NotFound);
        }

        registration.Course.CancelEnrollment(registration.Enrollment);
        db.Enrollments.Remove(registration.Enrollment);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(EnrollmentErrors.ConcurrencyConflict);
        }
    }
}
