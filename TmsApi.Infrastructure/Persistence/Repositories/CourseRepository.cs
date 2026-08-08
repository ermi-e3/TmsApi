// using Microsoft.EntityFrameworkCore;
// using TmsApi.Application.Interfaces;
// using TmsApi.Domain.Entities;

// namespace TmsApi.Infrastructure.Persistence.Repositories;

// public class CourseRepository(TmsDbContext context) : ICourseRepository
// {
//     public async Task<Course?> GetByCodeAsync(string courseCode, CancellationToken ct = default)
//     {
//         return await context
//             .Courses.Include(c => c.Enrollments)
//             .FirstOrDefaultAsync(c => c.Code == courseCode, ct);
//     }

//     public async Task<Course?> GetByIdAsync(int id, CancellationToken ct = default)
//     {
//         return await context
//             .Courses.Include(c => c.Enrollments)
//             .FirstOrDefaultAsync(c => c.Id == id, ct);
//     }

//     public async Task<bool> ExistsAsync(string courseCode, CancellationToken ct = default)
//     {
//         return await context.Courses.AnyAsync(c => c.Code == courseCode, ct);
//     }
// }

using Microsoft.EntityFrameworkCore;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Repositories;

public class CourseRepository(TmsDbContext context) : ICourseRepository
{
    public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context
            .Courses.Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Course?> GetByCodeAsync(string code, CancellationToken ct)
    {
        return await context
            .Courses.Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct)
    {
        return await context.Courses.AnyAsync(c => c.Code == code, ct);
    }

    public async Task AddAsync(Course course, CancellationToken ct)
    {
        await context.Courses.AddAsync(course, ct);
    }

    public Task DeleteAsync(Course course, CancellationToken ct)
    {
        context.Courses.Remove(course);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<Course>> GetAllAsync(CancellationToken ct)
    {
        return await context
            .Courses.AsNoTracking()
            .Include(c => c.Enrollments)
            .OrderBy(c => c.Title)
            .ToListAsync(ct);
    }

    public async Task<PagedResponse<CourseResponseDto>> GetPagedAsync(
        PagedRequest request,
        CancellationToken ct
    )
    {
        IQueryable<Course> query = context.Courses.AsNoTracking().Include(c => c.Enrollments);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{request.Search}%")
                || EF.Functions.ILike(c.Code, $"%{request.Search}%")
            );
        }

        var totalCount = await query.CountAsync(ct);

        query = request.OrderBy switch
        {
            "Code" => request.Descending
                ? query.OrderByDescending(c => c.Code)
                : query.OrderBy(c => c.Code),

            "MaxCapacity" => request.Descending
                ? query.OrderByDescending(c => c.MaxCapacity)
                : query.OrderBy(c => c.MaxCapacity),

            _ => request.Descending
                ? query.OrderByDescending(c => c.Title)
                : query.OrderBy(c => c.Title),
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count
            ))
            .ToListAsync(ct);

        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task UpdateAsync(Course course, CancellationToken ct)
    {
        context.Courses.Update(course);

        await Task.CompletedTask;
    }

    Task<CourseResponseDto?> ICourseRepository.GetByIdAsync(int id, CancellationToken ct) =>
        context
            .Courses.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count
            ))
            .FirstOrDefaultAsync(ct);

    // public Task<Course?> GetByCodeAsync(string code, CancellationToken ct)
    // {
    //     throw new NotImplementedException();
    // }

    public async Task<List<Course>> SearchAsync(string? term, CancellationToken ct)
    {
        IQueryable<Course> query = context.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            term = term.Trim();

            query = query.Where(c =>
                EF.Functions.ILike(c.Code, $"%{term}%") || EF.Functions.ILike(c.Title, $"%{term}%")
            );
        }

        return await query.OrderBy(c => c.Title).ToListAsync(ct);
    }
}
