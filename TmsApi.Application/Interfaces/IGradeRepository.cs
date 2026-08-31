using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IGradeRepository
{
    Task<Grade?> GetByStudentAndAssessmentAsync(
        int studentId,
        int assessmentId,
        CancellationToken ct
    );

    Task AddAsync(Grade grade, CancellationToken ct);
    Task<Assessment?> CheckAssesmentById(int id, CancellationToken ct);
}
