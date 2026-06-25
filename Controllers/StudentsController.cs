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
    private readonly TmsDbContext _context = context;

    [HttpGet("pageStudents")]
    public async Task<IActionResult> GetStudents(CancellationToken cancellationToken = default)
    {
        const int pageSize = 5,
            pageNumber = 1;

        var students = await _context
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
