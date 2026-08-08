using TmsApi.Application.DTOs;
using TmsApi.Application.Features.Courses.Commands.UpdateCourse;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct
    );

    Task<Course?> GetByCodeAsync(string code, CancellationToken ct);

    Task<List<Course>> GetAllAsync(CancellationToken ct);

    Task<CourseResponseDto?> UpdateAsync(UpdateCourseCommand command, CancellationToken ct);
}
