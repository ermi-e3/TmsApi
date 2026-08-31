using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Score).IsRequired().HasPrecision(5, 2);

        builder.Property(g => g.GradedAt).IsRequired();

        // Grade -> Student
        builder
            .HasOne(g => g.Student)
            .WithMany()
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Grade -> Assessment
        builder
            .HasOne(g => g.Assessment)
            .WithMany()
            .HasForeignKey(g => g.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // One grade per student per assessment
        builder.HasIndex(g => new { g.StudentId, g.AssessmentId }).IsUnique();
    }
}
