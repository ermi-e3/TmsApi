using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Models;

namespace TmsApi.Services;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService, TmsDbContext context) : ControllerBase
{
    private readonly TmsDbContext _context = context;

    [HttpGet("top5-courses")]
    public async Task<IActionResult> GetTopCourses(CancellationToken cancellationToken)
    {
        var result = await _context
            .Courses.Select(c => new { c.Title, EnrollmentCount = c.Enrollments.Count })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    // [HttpGet("honor-students-count")]
    // public async Task<IActionResult> GetHonorStudentsCount(CancellationToken cancellationToken)
    // {
    //     var count = await _context
    //         .Students.Where(s => s.IsActive && s.GPA >= 3.0m)
    //         .CountAsync(cancellationToken);

    //     return Ok(count);
    // }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await courseService.GetAllAsync();

        return Ok(courses);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetById(string code)
    {
        var course = await courseService.GetByIdAsync(code);

        return course is not null ? Ok(course) : NotFound();
    }

    // [HttpPost]
    // public async Task<IActionResult> Create(
    //     CreateCourseRequest request)
    // {
    //     var course = await courseService.CreateAsync(

    //         request.Code,
    //         request.Title,
    //         request.Capacity);

    //     return CreatedAtAction(
    //         nameof(GetById),
    //         new { code = course.Code },
    //         course);
    // }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var deleted = await courseService.DeleteAsync(code);

        return deleted ? NoContent() : NotFound();
    }
}
