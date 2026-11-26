using Microsoft.AspNetCore.Mvc;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Pagination;
using WebAPI.Common.Results;
using WebAPI.Features.Courses.CreateCourse;
using WebAPI.Features.Courses.GetCourse;
using WebAPI.Features.Courses.ListCourses;

namespace WebAPI.Features.Courses;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Create a course")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(
        CreateCourseCommand command,
        [FromServices] ICommandHandler<CreateCourseCommand, Result<Guid>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, result.Value)
            : result.ToProblemDetails(HttpContext);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a course by ID")]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResponse>> Get(
        Guid id,
        [FromServices] IQueryHandler<GetCourseQuery, Result<CourseResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetCourseQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }

    [HttpGet]
    [EndpointSummary("List courses")]
    [ProducesResponseType<PagedResult<CourseResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CourseResponse>>> List(
        [FromServices] IQueryHandler<ListCoursesQuery, Result<PagedResult<CourseResponse>>> handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = Pagination.DefaultPage,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var result = await handler.HandleAsync(new ListCoursesQuery(page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }
}
