using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Application.Services;

public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
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

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct
    )
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity,
        };
        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);

        return (await GetByIdAsync(course.Id, ct))!;
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct
    )
    {
        // TODO 1: Start with a no-tracking IQueryable<Course>:
        IQueryable<Course> query = context.Courses.AsNoTracking();

        // TODO 2: If request.Search has a value, append a Where clause:
        // query = query.Where(c =>
        //     EF.Functions.ILike(c.Title, $"%{request.Search}%")
        //     || EF.Functions.ILike(c.Code, $"%{request.Search}%")
        // );
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{request.Search}%")
                || EF.Functions.ILike(c.Code, $"%{request.Search}%")
            );
        }

        // ILike is the case-insensitive LIKE in PostgreSQL using it here means
        // the search "fund" finds "Web Development Fundamentals" without learners
        // being surprised by case-sensitivity at lab time.

        // TODO 3: Count BEFORE paging:
        var totalCount = await query.CountAsync(ct);
        // This produces one SELECT COUNT(*) statement. If you Count after Skip/Take,
        // you would get the count of the page, not the total.

        // TODO 4: Apply OrderBy, then Skip/Take, then Select projection.
        // For OrderBy, branch on request.OrderBy ∈ { "Title", "Code", "MaxCapacity" }
        // and apply Descending if request.Descending. Reject unknown OrderBy values
        // silently by falling back to "Title" never let an arbitrary string
        // into the LINQ tree.
        IQueryable<Course> sortedQuery = request.OrderBy switch
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

        // TODO 5: Materialise:
        // var items = await sortedQuery
        //.Skip((request.Page- 1) * request.PageSize)
        //.Take(request.PageSize)
        // .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
        //.ToListAsync(ct);
        var items = await sortedQuery
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

        // TODO 6: Return new PagedResponse<CourseResponseDto> { Items = items, TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
