using TmsApi.Dtos;
using TmsApi.Models;

namespace TmsApi.Services;

public interface IStudentService
{
    // Task<Student> CreateAsync(
    //     int id,
    //     string registrationNumber,
    //     string name,
    //     decimal gpa,
    //     bool isActive,
    //     ICollection<Enrollment> enrollments
    // );

    // Task<Student?> GetByIdAsync(string id);

    // Task<IReadOnlyList<Student>> GetAllAsync(PagedRequest request, CancellationToken ct);

    // Task<bool> DeleteAsync(string id);

    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);
    Task<bool> RegistrationNoExistsAsync(string code, CancellationToken ct);
    Task<PagedResponse<StudentResponseDto>> GetStudentAsync(
        PagedRequest request,
        CancellationToken ct
    );

    Task<StudentResponseDto?> UpdateAsync(
        int id,
        UpdateStudentRequest request,
        CancellationToken ct
    );

    Task<StudentResponseDto?> PatchAsync(int id, PatchStudentRequest request, CancellationToken ct);        
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
