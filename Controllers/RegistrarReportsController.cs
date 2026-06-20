using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class RegistrarReportsController : ControllerBase
{
    private readonly TmsDbContext _context;

    public RegistrarReportsController(TmsDbContext context)
    {
        _context = context;
    }

    [HttpGet("honor-students-count")]
    public async Task<IActionResult> GetHonorStudentsCount()
    {
        var count = await _context.Students.Where(s => s.IsActive && s.GPA >= 3.0m).CountAsync();

        return Ok(count);
    }

    [HttpGet("courses-by-enrollment-descending")]
    public async Task<IActionResult> GetCoursesByEnrollment()
    {
        var list = await _context
            .Courses.Select(c => new { c.Title, EnrollmentCount = c.Enrollments.Count })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> GetAverageGpaPerCourse()
    {
        var list = await _context
            .Enrollments.GroupBy(e => e.Course.Title)
            .Select(g => new { Course = g.Key, AverageGPA = g.Average(e => e.Student.GPA) })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("students-without-enrollments")]
    public async Task<IActionResult> GetStudentsWithoutEnrollmentsSubquery()
    {
        var list = await _context
            .Students.Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("students-without-enrollments-leftjoin")]
    public async Task<IActionResult> GetStudentsWithoutEnrollmentsLeftJoin()
    {
        var list = await _context
            .Students.LeftJoin(
                _context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e }
            )
            .Where(x => x.e == null)
            .Select(x => x.s.Name)
            .ToListAsync();

        return Ok(list);
    }
}
