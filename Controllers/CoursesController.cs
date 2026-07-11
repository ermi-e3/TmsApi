using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(ICourseService courseService, LinkGenerator linkGenerator)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription(
        "Returns a paginated, optionally filtered listof TMS courses. PageSize is capped at 50."
    )]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct
    )
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    // [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    // public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    // {
    //     var course = await courseService.GetByIdAsync(id, ct);
    //     return course is not null ? Ok(course) : NotFound();
    // }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription(
        "Returns course details with HATEOAS links. Returns 404 if the course does not exist."
    )]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        // var course = await courseService.GetByIdAsync(id, ct);
        // if (course is null)
        //     return NotFound();

        // TODO 1: Use linkGenerator.GetPathByName(HttpContext, routeName, values) to build each href.
        //The route names are the ones you set with `Name = nameof(...)` on the actions:
        //- nameof(GetCourseById)- nameof(GetEnrollment)for /api/courses/{id}
        // - nameof(GetEnrollment) for /api/courses/{courseId}/enrollments/{id}
        //For the list-enrolments link, you will need to build the path manually with
        // linkGenerator.GetPathByAction(HttpContext, action: "GetEnrollments",
        //controller: "Enrollments",values: new { courseId = id })
        //OR add Name = "ListCourseEnrollments" on the [HttpGet] of EnrollmentsController and use GetPathByName.

        // TODO 2: Build a List<LinkDto> with these entries:
        //- { rel: "self", method: "GET", href: GetCourseById name with { id } }
        //- { rel: "update", method: "PUT", href: GetCourseById name with { id } }
        //- { rel: "delete", method: "DELETE", href: GetCourseById name with { id } }
        //- { rel: "enrollments", method: "GET", href: <list-enrollments path> }
        // If course.EnrollmentCount < course.MaxCapacity, also add:
        //- { rel: "enroll", method: "POST", href: <list-enrollments path> }
        // The conditional link is what makes HATEOAS earn its cost the Angular
        //team uses its presence/absence to render or hide the Enrol button without
        //duplicating the capacity rule in TypeScript.
        // TODO 3: Build a CourseDetailDto from `course` plus the Links list and return Ok(detailDto).

        var course = await courseService.GetByIdAsync(id, ct);

        if (course is null)
        {
            return NotFound();
        }

        var selfHref = linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });

        var enrollmentsHref = linkGenerator.GetPathByName(
            HttpContext,
            "ListCourseEnrollments",
            new { courseId = id }
        );

        var links = new List<LinkDto>
        {
            new(selfHref!, "self", "GET"),
            new(selfHref!, "update", "PUT"),
            new(selfHref!, "delete", "DELETE"),
            new(enrollmentsHref!, "enrollments", "GET"),
        };

        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new LinkDto(enrollmentsHref!, "enroll", "POST"));
        }

        var detail = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links,
        };

        return Ok(detail);
        // throw new NotImplementedException();
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription(
        "Creates a course with a unique code. Returns 409 if the course code already exists."
    )]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        // var result = await courseService.CreateAsync(request, ct);
        // return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);

        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Course code already exists",
                    Detail = $"A course with code '{request.Code}' is already registered.",
                    Status = StatusCodes.Status409Conflict,
                }
            );
        }

        var result = await courseService.CreateAsync(request, ct);

        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }
}
