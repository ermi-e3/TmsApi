using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Entities;

public class StudentService(TmsDbContext context, ILogger<StudentService> _logger) : IStudentService
{
    private readonly Dictionary<string, Student> _store = new();

    // {
    //     _store.TryGetValue(id, out var student);

    //     if (student is null)
    //     {
    //         _logger.LogWarning("Student {StudentId} not found", id);
    //     }

    //     return Task.FromResult(student);
    // }

    public Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context
            .Students.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new StudentResponseDto(
                c.Id,
                c.RegistrationNumber,
                c.Name,
                c.Age,
                c.GPA,
                c.IsActive,
                c.Enrollments.Count
            ))
            .FirstOrDefaultAsync(ct);

    public async Task<PagedResponse<StudentResponseDto>> GetStudentAsync(
        PagedRequest request,
        CancellationToken ct
    )
    {
        IQueryable<Student> query = context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Name, $"%{request.Search}%")
                || EF.Functions.ILike(c.Name, $"%{request.Search}%")
            );
        }

        var totalCount = await query.CountAsync(ct);

        IQueryable<Student> sortedQuery = request.OrderBy switch
        {
            "Name" => request.Descending
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),

            "GPA" => request.Descending
                ? query.OrderByDescending(c => c.GPA)
                : query.OrderBy(c => c.GPA),

            "Age" => request.Descending
                ? query.OrderByDescending(c => c.Age)
                : query.OrderBy(c => c.Age),

            _ => request.Descending
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
        };

        var items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new StudentResponseDto(
                c.Id,
                c.RegistrationNumber,
                c.Name,
                c.Age,
                c.GPA,
                c.IsActive,
                c.Enrollments.Count
            ))
            .ToListAsync(ct);

        return new PagedResponse<StudentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task<StudentResponseDto> CreateAsync(
        CreateStudentRequest request,
        CancellationToken ct
    )
    {
        var student = new Student
        {
            Name = request.Name,
            RegistrationNumber = request.Name,
            GPA = request.GPA,
        };
        context.Students.Add(student);

        await context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created course {CourseId} ({Code})",
            student.Id,
            student.RegistrationNumber
        );

        return (await GetByIdAsync(student.Id, ct))!;
    }

    public Task<bool> RegistrationNoExistsAsync(string registrationNo, CancellationToken ct) =>
        context.Students.AsNoTracking().AnyAsync(c => c.RegistrationNumber == registrationNo, ct);
}
