using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Results;
using WebAPI.Database;

namespace WebAPI.Features.Enrollments.GetEnrollment;

internal sealed class GetEnrollmentQueryHandler(SchoolDbContext db)
    : IQueryHandler<GetEnrollmentQuery, Result<EnrollmentResponse>>
{
    public async Task<Result<EnrollmentResponse>> HandleAsync(
        GetEnrollmentQuery query, CancellationToken cancellationToken)
    {
        var enrollment = await db.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.StudentId == query.StudentId && enrollment.CourseId == query.CourseId)
            .Select(enrollment => new EnrollmentResponse(enrollment.StudentId, enrollment.CourseId, enrollment.EnrolledAt))
            .SingleOrDefaultAsync(cancellationToken);

        return enrollment is null
            ? Result<EnrollmentResponse>.Failure(EnrollmentErrors.NotFound)
            : Result<EnrollmentResponse>.Success(enrollment);
    }
}
