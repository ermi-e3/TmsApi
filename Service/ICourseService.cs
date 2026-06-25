using TmsApi.Models;

namespace TmsApi.Entities;

public interface ICourseService
{
    Task<Course> CreateAsync(
        int id,
        string code,
        string title,
        int capacity,
        ICollection<Enrollment> enrollments
    );

    Task<Course?> GetByIdAsync(string code);

    Task<IReadOnlyList<Course>> GetAllAsync();

    Task<bool> DeleteAsync(string code);
}
