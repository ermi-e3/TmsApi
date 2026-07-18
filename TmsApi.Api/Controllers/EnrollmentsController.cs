// using Microsoft.AspNetCore.Mvc;
// using TmsApi.Application.DTOs;
// using TmsApi.Application.Interfaces;

// namespace TmsApi.Api.Controllers;

// [ApiController]
// [Route("api/courses/{courseId:int}/enrollments")]
// [Tags("Enrollments")]
// [Produces("application/json")]
// [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
// public class EnrollmentsController(
//     ICourseService courseService,
//     IEnrollmentService enrollmentService
// ) : ControllerBase
// {
//     [HttpGet(Name = "ListCourseEnrollments")]
//     [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>), StatusCodes.Status200OK)]
//     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//     [EndpointSummary("List enrolments for a course")]
//     public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
//     {
//         var course = await courseService.GetByIdAsync(courseId, ct);

//         if (course is null)
//         {
//             return NotFound();
//         }

//         var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);

//         return Ok(enrollments);
//     }

//     ////////////////////////////////////////////////////////

//     [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
//     [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
//     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//     [EndpointSummary("Get one enrolment for a course")]
//     public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
//     {
//         var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
//         return enrollment is not null ? Ok(enrollment) : NotFound();
//     }

//     [HttpPost]
//     [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
//     [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
//     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
//     [EndpointSummary("Enrol a student in a course")]
//     [EndpointDescription(
//         "Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity."
//     )]
//     public async Task<IActionResult> EnrollStudent(
//         int courseId,
//         EnrollStudentRequest request,
//         CancellationToken ct
//     )
//     {
//         var course = await courseService.GetByIdAsync(courseId, ct);

//         if (course is null)
//         {
//             return NotFound();
//         }

//         if (course.EnrollmentCount >= course.MaxCapacity)
//         {
//             return Conflict(
//                 new ProblemDetails
//                 {
//                     Title = "Course is full",
//                     Detail =
//                         $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
//                     Status = StatusCodes.Status409Conflict,
//                 }
//             );
//         }

//         var enrollment = await enrollmentService.CreateAsync(courseId, request, ct);

//         return CreatedAtAction(
//             nameof(GetEnrollment),
//             new { courseId, id = enrollment.Id },
//             enrollment
//         );
//     }
// }

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Enroll(EnrollStudentCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.Match<IActionResult>(
            onSuccess: created =>
                CreatedAtAction(
                    nameof(GetSchedule),
                    new { studentId = created.StudentId },
                    created
                ),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" => StatusCodes.Status404NotFound,
                    "course_full" or "already_enrolled" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest,
                };
                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}"
                );
            }
        );
    }

    [HttpGet("{studentId}/schedule")]
    public async Task<IActionResult> GetSchedule(int studentId, CancellationToken ct)
    {
        var schedule = await mediator.Send(new GetStudentScheduleQuery(studentId), ct);
        return Ok(schedule);
    }

    
}


