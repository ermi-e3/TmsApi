using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
// using TmsApi.Application.Grades;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Repositories;

public class GradeRepository(TmsDbContext db) : IGradeRepository
{
    public Task<Grade?> GetByStudentAndAssessmentAsync(
        int studentId,
        int assessmentId,
        CancellationToken ct
    )
    {
        return db.Grades.FirstOrDefaultAsync(
            g => g.StudentId == studentId && g.AssessmentId == assessmentId,
            ct
        );
    }

    public async Task AddAsync(Grade grade, CancellationToken ct)
    {
        db.Grades.Add(grade);
        await db.SaveChangesAsync(ct);
    }

    public async Task<Assessment?> CheckAssesmentById(int id, CancellationToken ct)
    {
        var assessment = await db.Assessment.Where(c => c.Id == id).FirstOrDefaultAsync(ct);

        if (assessment is null)
            return null;

        return assessment;
    }
}
