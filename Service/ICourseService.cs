
using TmsApi.Models;

public interface ICourseService
{
    Task<Course> CreateAsync(
        string code,
        string title,
        int capacity);

    Task<Course?> GetByIdAsync(string code);

    Task<IReadOnlyList<Course>> GetAllAsync();

    Task<bool> DeleteAsync(string code);
}