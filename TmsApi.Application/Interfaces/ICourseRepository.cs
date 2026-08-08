using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

// public interface ICourseRepository
// {
//     Task<Course?> GetByCodeAsync(string courseCode, CancellationToken ct = default);

//     Task<Course?> GetByIdAsync(int id, CancellationToken ct = default);

//     Task<bool> ExistsAsync(string courseCode, CancellationToken ct = default);
// }

public interface ICourseRepository
{
    Task<CourseResponseDto> GetByIdAsync(int id, CancellationToken ct);

    Task<Course?> GetByCodeAsync(string code, CancellationToken ct);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    Task AddAsync(Course course, CancellationToken ct);

    Task DeleteAsync(Course course, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);

    Task<List<Course>> GetAllAsync(CancellationToken ct);

    Task<PagedResponse<CourseResponseDto>> GetPagedAsync(
        PagedRequest request,
        CancellationToken ct
    );
    Task UpdateAsync(Course course, CancellationToken ct);
}
