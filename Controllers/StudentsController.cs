using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Models;

namespace TmsApi.Services;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService, TmsDbContext context)
    : ControllerBase
{
    [HttpGet("all-student-deleted")]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await context.Students.IgnoreQueryFilters().ToListAsync();

        return Ok(students);
    }

    [HttpGet("studentCount")]
    public async Task<IActionResult> GetStudentsCount(CancellationToken cancellationToken = default)
    {
        // N + 1  pattern
        // var students = await context.Students.AsNoTracking().ToListAsync(cancellationToken);
        // foreach (var s in students)
        // {
        //     // TODO: Query enrollment count for this student inside the loop (use StudentId).
        //     // This should produce 1 + N SQL statements. Count them in the log.

        //     var count = await context
        //         .Enrollments.AsNoTracking()
        //         .CountAsync(e => e.StudentId == s.Id, cancellationToken);
        //     Console.WriteLine($"{s.Name}: {count} enrollments");
        // }

        ////////////////////////////////////////////////////////////////////////////////////////

        //  FIX: Single query with projection
        var students = await context
            .Students.AsNoTracking()
            .Select(s => new { s.Name, EnrollmentCount = s.Enrollments.Count })
            .ToListAsync(cancellationToken);

        foreach (var r in students)
            Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments");

        ////////////////////////////////////////////////////////////////////////////////////////

        // FIX: using include
        // BUG: this throw JSON serialization error
        // var students = await context
        //     .Students.AsNoTracking()
        //     .Include(s => s.Enrollments)
        //     .ToListAsync(cancellationToken);

        // foreach (var s in students)
        //     Console.WriteLine($"{s.Name}: {s.Enrollments.Count} enrollments");

        return Ok(students);
    }

    [HttpGet("pageStudents")]
    public async Task<IActionResult> GetStudents(CancellationToken cancellationToken = default)
    {
        const int pageSize = 5,
            pageNumber = 1;

        var students = await context
            .Students.OrderBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await studentService.GetAllAsync();

        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var student = await studentService.GetByIdAsync(id);

        return student is not null ? Ok(student) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentRequest request)
    {
        var student = await studentService.CreateAsync(
            request.Id,
            request.RegistrationNumber,
            request.Name,
            request.GPA,
            request.IsActive,
            request.enrollments
        );

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}
