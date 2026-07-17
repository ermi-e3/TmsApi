using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Student> Students { get; }

    DbSet<Course> Courses { get; }

    DbSet<Enrollment> Enrollments { get; }

    DbSet<Assessment> Assessment { get; }

    DbSet<Certificate> Certificate { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
