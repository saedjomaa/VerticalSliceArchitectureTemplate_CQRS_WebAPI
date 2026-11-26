using Microsoft.AspNetCore.Mvc;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Pagination;
using WebAPI.Common.Results;
using WebAPI.Features.Enrollments.CancelEnrollment;
using WebAPI.Features.Enrollments.EnrollStudent;
using WebAPI.Features.Enrollments.GetEnrollment;
using WebAPI.Features.Enrollments.ListStudentCourses;

namespace WebAPI.Features.Enrollments;

[ApiController]
[Route("api/students/{studentId:guid}")]
public sealed class EnrollmentsController : ControllerBase
{
    private const string EnrollmentRoute = "enrollments/{courseId:guid}";

    [HttpPost("enrollments")]
    [EndpointSummary("Create a course enrollment for a student")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid studentId,
        EnrollStudentRequest request,
        [FromServices] ICommandHandler<EnrollStudentCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new EnrollStudentCommand(studentId, request.CourseId), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblemDetails(HttpContext);
    }

    [HttpGet(EnrollmentRoute)]
    [EndpointSummary("Get a student's enrollment in a course")]
    [ProducesResponseType<EnrollmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrollmentResponse>> Get(
        Guid studentId,
        Guid courseId,
        [FromServices] IQueryHandler<GetEnrollmentQuery, Result<EnrollmentResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetEnrollmentQuery(studentId, courseId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }

    [HttpDelete(EnrollmentRoute)]
    [EndpointSummary("Cancel a student's enrollment and release the seat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        Guid studentId,
        Guid courseId,
        [FromServices] ICommandHandler<CancelEnrollmentCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new CancelEnrollmentCommand(studentId, courseId), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblemDetails(HttpContext);
    }

    [HttpGet("courses")]
    [EndpointSummary("List courses a student is enrolled in")]
    [ProducesResponseType<PagedResult<StudentCourseResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<StudentCourseResponse>>> ListEnrolledCourses(
        Guid studentId,
        [FromServices] IQueryHandler<ListStudentCoursesQuery, Result<PagedResult<StudentCourseResponse>>> handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = Pagination.DefaultPage,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var result = await handler.HandleAsync(new ListStudentCoursesQuery(studentId, page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }
}
