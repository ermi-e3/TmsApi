using TmsApi.Models;
namespace TmsApi.Entities;


public interface IStudentService
{
    Task<Student> CreateAsync(
        int id,
        string registrationNumber,
        string name,
        decimal gpa,
        bool isActive,
        ICollection<Enrollment> enrollments);

    Task<Student?> GetByIdAsync(string id);

    Task<IReadOnlyList<Student>> GetAllAsync();

    Task<bool> DeleteAsync(string id);
}