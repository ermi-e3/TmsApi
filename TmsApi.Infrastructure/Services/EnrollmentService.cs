using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Application.Services;

public class EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger)
    : IEnrollmentService
{
    public Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct) =>
        context
            .Enrollments.AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.Course.Title,
                e.StudentId,
                e.Student.Name,
                e.EnrolledAt,
                e.Status
            ))
            .FirstOrDefaultAsync(ct);

    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    )
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow,
        };

        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Student {StudentId} enrolled in course {CourseId}",
            enrollment.StudentId,
            enrollment.CourseId
        );

        return await GetByIdAsync(courseId, enrollment.Id, ct)
            ?? throw new InvalidOperationException(
                "Enrollment was created but could not be retrieved."
            );
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct
    )
    {
        return await context
            .Enrollments.AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.Course.Title,
                e.StudentId,
                e.Student.Name,
                e.EnrolledAt,
                e.Status
            ))
            .ToListAsync(ct);
    }

    public async Task ApproveAsync(int id, CancellationToken ct)
    {
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (enrollment is null)
        {
            return;
        }

        enrollment.Status = "Approved";

        await context.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(int id, CancellationToken ct)
    {
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (enrollment is null)
        {
            return;
        }

        enrollment.Status = "Rejected";

        await context.SaveChangesAsync(ct);
    }
    
    public async Task<EnrollmentResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context
            .Enrollments.Where(e => e.Id == id)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.Course.Title,
                e.StudentId,
                e.Student.Name,
                e.EnrolledAt,
                e.Status
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<EnrollmentResponseDto>> GetAllAsync(CancellationToken ct)
    {
        return await context
            .Enrollments.Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.Course.Title,
                e.StudentId,
                e.Student.Name,
                e.EnrolledAt,
                e.Status
            ))
            .ToListAsync(ct);
    }

}
