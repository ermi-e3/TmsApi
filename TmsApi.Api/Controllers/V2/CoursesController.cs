using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
using TmsApi.Application.DTOs;
using TmsApi.Application.Features.Courses.Commands.CreateCourse;
using TmsApi.Application.Features.Courses.Commands.UpdateCourse;
using TmsApi.Application.Features.Courses.Queries.GetCourse;
using TmsApi.Application.Features.Courses.Queries.GetCourseById;
using TmsApi.Application.Features.Courses.Queries.GetCourses;
using TmsApi.Application.Interfaces;

// using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(IMediator mediator) : ControllerBase // impliments IApplicationDbContext interface not using TmsDbContext
{
    // [HttpGet]
    // public async Task<IActionResult> GetCourses(
    //     [FromQuery] int page = 1,
    //     [FromQuery] int pageSize = 20,
    //     CancellationToken ct = default
    // )
    // {
    //     page = Math.Max(1, page);
    //     pageSize = Math.Clamp(pageSize, 1, 50);

    //     var baseQuery = context.Courses.AsNoTracking();

    //     var totalCount = await baseQuery.CountAsync(ct);

    //     var rows = await baseQuery
    //         .OrderBy(c => c.Title)
    //         .Skip((page - 1) * pageSize)
    //         .Take(pageSize)
    //         .Select(c => new
    //         {
    //             c.Id,
    //             c.Title,
    //             c.Code,
    //             c.MaxCapacity,
    //             EnrollmentCount = c.Enrollments.Count,
    //         })
    //         .ToListAsync(ct);

    //     var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    //     var hasNext = page < totalPages;
    //     var hasPrevious = page > 1;

    //     return Ok(
    //         new
    //         {
    //             data = rows,
    //             meta = new
    //             {
    //                 totalCount,
    //                 page,
    //                 pageSize,
    //                 totalPages,
    //                 hasNext,
    //                 hasPrevious,
    //             },
    //             links = new
    //             {
    //                 self = $"/api/v2/courses?page={page}&pageSize={pageSize}",
    //                 next = hasNext
    //                     ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}"
    //                     : (string?)null,
    //                 prev = hasPrevious
    //                     ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}"
    //                     : (string?)null,
    //                 enroll = "/api/v2/enrollments",
    //             },
    //         }
    //     );
    // }

    [HttpGet("{code}")]
    public async Task<ActionResult<CourseResponseDto>> GetCourse(string code, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCourseQuery(code), ct);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CourseResponseDto>>> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct
    )
    {
        var result = await mediator.Send(new GetCoursesQuery(request), ct);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseResponseDto>> GetById(int id, CancellationToken ct)
    {
        var course = await mediator.Send(new GetCourseByIdQuery(id), ct);

        return Ok(course);
    }

    [HttpPost]
    public async Task<ActionResult<CourseResponseDto>> Create(
        CreateCourseRequest request,
        CancellationToken ct
    )
    {
        var course = await mediator.Send(
            new CreateCourseCommand(request.Code, request.Title, request.MaxCapacity),
            ct
        );

        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CourseResponseDto>> Update(
        int id,
        [FromBody]UpdateCourseRequest request,
        CancellationToken ct
    )
    {
        var command = new UpdateCourseCommand(id, request.Code, request.Title, request.MaxCapacity);

        var result = await mediator.Send(command, ct);

        return Ok(result);
    }
}
