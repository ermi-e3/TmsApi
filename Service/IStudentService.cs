using TmsApi.Models;

public interface IStudentService
{
    Task<Student> CreateAsync(
        string id,
        string name,
        int age,
        decimal gpa);

    Task<Student?> GetByIdAsync(string id);

    Task<IReadOnlyList<Student>> GetAllAsync();

    Task<bool> DeleteAsync(string id);
}