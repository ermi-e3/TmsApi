using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IAssessmentRepository
{
    Task<Assessment?> GetByIdAsync(
        int id,
        CancellationToken ct = default);
}