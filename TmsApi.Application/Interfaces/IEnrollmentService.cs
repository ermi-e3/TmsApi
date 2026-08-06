using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    );
    Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);

    Task ApproveAsync(int id, CancellationToken ct);
    Task RejectAsync(int id, CancellationToken ct);

    Task<EnrollmentResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<IEnumerable<EnrollmentResponseDto>> GetAllAsync(CancellationToken ct);
}
