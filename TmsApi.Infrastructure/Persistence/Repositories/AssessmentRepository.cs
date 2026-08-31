using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Repositories;

public class AssessmentRepository(TmsDbContext db) : IAssessmentRepository
{
    public async Task<Assessment?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Assessment.FirstOrDefaultAsync(a => a.Id == id, ct);
    }
}
