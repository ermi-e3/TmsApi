using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;

namespace TmsApi.Infrastructure.Persistence.Repositories;

public class StudentRepository(TmsDbContext db) : IStudentRepository
{
    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await db.Students.AnyAsync(s => s.Id == id, ct);
    }
}
