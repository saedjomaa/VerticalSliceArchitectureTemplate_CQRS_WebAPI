using WebAPI.Common.Results;

namespace WebAPI.Features.Students;

internal static class StudentErrors
{
    public static readonly Error NotFound = new(
        "Students.NotFound", "The student was not found.", ErrorType.NotFound);
}
