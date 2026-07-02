using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

// public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
// {
//     public DbSet<Student> Students => Set<Student>();
//     public DbSet<Course> Courses => Set<Course>();
//     public DbSet<Enrollment> Enrollments => Set<Enrollment>();
//     public DbSet<Assessment> Assessment => Set<Assessment>();
//     public DbSet<Certificate> Certificate => Set<Certificate>();
// }

public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Assessment> Assessment => Set<Assessment>();

    public DbSet<Certificate> Certificate => Set<Certificate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) // this method comes from DbContext and replace the parent implementation with my own version.
    {
        //         EntityEntry
        //         ├── Entity
        //         ├── State
        //         ├── OriginalValues
        //         ├── CurrentValues
        //         └── Metadata
        // state
        //   ├──Added
        //      Modified
        //      Deleted
        //      Unchanged
        //      Detached

        //
        //     foreach (var entry in ChangeTracker.Entries<Student>()) // EF Core watches every entity it loads: ChangeTracker
        //     // chnge tracker track student entity
        //     {
        //         if (entry.State == EntityState.Added || entry.State == EntityState.Modified) //this will check if the student entity has add new data or update
        //         {
        //             entry.Property("LastUpdated").CurrentValue = DateTime.UtcNow;
        //         }
        //     }

        //     return await base.SaveChangesAsync(cancellationToken);
        // }

        foreach (var entry in ChangeTracker.Entries<Student>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Property("LastUpdated").CurrentValue = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
