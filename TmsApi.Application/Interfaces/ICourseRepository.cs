using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByCodeAsync(string courseCode, CancellationToken ct = default);

    Task<Course?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> ExistsAsync(string courseCode, CancellationToken ct = default);
}
