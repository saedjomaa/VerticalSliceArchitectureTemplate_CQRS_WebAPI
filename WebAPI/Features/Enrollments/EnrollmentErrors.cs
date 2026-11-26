using WebAPI.Common.Results;

namespace WebAPI.Features.Enrollments;

internal static class EnrollmentErrors
{
    public static readonly Error NotFound = new(
        "Enrollments.NotFound", "The enrollment was not found.", ErrorType.NotFound);

    public static readonly Error AlreadyEnrolled = new(
        "Enrollments.AlreadyEnrolled", "The student is already enrolled in this course.", ErrorType.Conflict);

    public static readonly Error CourseFull = new(
        "Enrollments.CourseFull", "The course has reached its capacity.", ErrorType.Conflict);

    public static readonly Error ConcurrencyConflict = new(
        "Enrollments.ConcurrencyConflict", "The course enrollment changed during the request. Try again.", ErrorType.Conflict);
}
