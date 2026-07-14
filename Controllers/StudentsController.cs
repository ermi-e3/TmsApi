using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentsController(IStudentService studentService, LinkGenerator linkGenerator)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List Student with pagination")]
    [EndpointDescription(
        "Returns a paginated, optionally filtered listof TMS Student. PageSize is capped at 50."
    )]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct
    )
    {
        var result = await studentService.GetStudentAsync(request, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new Student")]
    [EndpointDescription(
        "Creates a Student with a unique Regidtration code. Returns 409 if the Regidtration code already exists."
    )]
    public async Task<IActionResult> CreateStudent(
        CreateStudentRequest request,
        CancellationToken ct
    )
    {
        // var result = await studentService.CreateAsync(request, ct);
        // return CreatedAtAction(nameof(GetStudentById), new { id = result.Id }, result);

        if (await studentService.RegistrationNoExistsAsync(request.RegistrationNumber, ct))
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Student Registration code already exists",
                    Detail =
                        $"A Student with Registration code '{request.RegistrationNumber}' is already registered.",
                    Status = StatusCodes.Status409Conflict,
                }
            );
        }

        var result = await studentService.CreateAsync(request, ct);

        return CreatedAtAction(nameof(GetStudentById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a student by ID")]
    [EndpointDescription(
        "Returns student details with HATEOAS links. Returns 404 if the student does not exist."
    )]
    public async Task<IActionResult> GetStudentById(int id, CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(id, ct);

        if (student is null)
        {
            return NotFound();
        }

        var selfHref = linkGenerator.GetPathByName(HttpContext, nameof(GetStudentById), new { id });

        var links = new List<LinkDto>
        {
            new(selfHref!, "self", "GET"),
            new(selfHref!, "post", "POST"),
            new(selfHref!, "update", "PUT"),
            new(selfHref!, "delete", "DELETE"),
        };

        var detail = new StudentDetailDto
        {
            Id = student.Id,
            RegistrationNumber = student.RegistrationNumber,
            Name = student.Name,
            Age = student.Age,
            GPA = student.GPA,
            IsActive = student.IsActive,
            EnrollmentCount = student.EnrollmentCount,
            Links = links,
        };

        return Ok(detail);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a student")]
    [EndpointDescription(
        "Updates an existing student. Returns the updated student if successful. Returns 404 if the student does not exist."
    )]
    public async Task<IActionResult> UpdateStudent(
        int id,
        UpdateStudentRequest request,
        CancellationToken ct
    )
    {
        var student = await studentService.UpdateAsync(id, request, ct);

        if (student is null)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "Student not found",
                    Detail = $"No student with ID '{id}' was found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );
        }

        return Ok(student);
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Partially update a student")]
    [EndpointDescription("Updates only the provided student fields.")]
    public async Task<IActionResult> PatchStudent(
        int id,
        PatchStudentRequest request,
        CancellationToken ct
    )
    {
        var student = await studentService.PatchAsync(id, request, ct);

        if (student is null)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "Student not found",
                    Detail = $"No student with ID '{id}' was found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );
        }

        return Ok(student);
    }

    [HttpDelete("{id:int}", Name = nameof(DeleteStudent))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a student by ID")]
    [EndpointDescription(
        "Deletes a student record permanently. Returns 204 No Content on success or 404 if the student does not exist."
    )]
    public async Task<IActionResult> DeleteStudent(int id, CancellationToken ct)
    {
        var deleted = await studentService.DeleteAsync(id, ct);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
