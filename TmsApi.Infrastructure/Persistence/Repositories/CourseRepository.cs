using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Repositories;

public class CourseRepository(TmsDbContext context) : ICourseRepository
{
    public async Task<Course?> GetByCodeAsync(string courseCode, CancellationToken ct = default)
    {
        return await context
            .Courses.Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == courseCode, ct);
    }

    public async Task<Course?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await context
            .Courses.Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<bool> ExistsAsync(string courseCode, CancellationToken ct = default)
    {
        return await context.Courses.AnyAsync(c => c.Code == courseCode, ct);
    }
}
