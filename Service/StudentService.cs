using TmsApi.Models;
namespace TmsApi.Entities;


public class StudentService :  IStudentService
{
    private readonly Dictionary<string, Student> _store = new();

    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public Task<Student> CreateAsync(
        int id,
        string registrationNumber,
        string name,
        decimal gpa,
        bool isActive,
        ICollection<Enrollment> enrollments)
    {
        _logger.LogInformation(
            "Student creation request received for {StudentId}",
            id);

        _logger.LogInformation(
            "Current student count: {Count}",
            _store.Count);

        // if (_store.TryGetValue(id, out var existing))
        // {
        //     _logger.LogWarning(
        //         "Duplicate student creation attempt {StudentId}",
        //         id);

        //     return Task.FromResult(existing);
        // }

        var student = new Student
        {
            Id = id,
            Name = name,
            RegistrationNumber = registrationNumber,
            GPA = gpa,
            IsActive=isActive,
            Enrollments = enrollments

        };

        // _store[id] = student;

        _logger.LogInformation(
            "Created student {StudentId}",
            id);

        _logger.LogInformation(
            "Student count after insert: {Count}",
            _store.Count);

        return Task.FromResult(student);
    }

    public Task<Student?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var student);

        if (student is null)
        {
            _logger.LogWarning(
                "Student {StudentId} not found",
                id);
        }

        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<Student>> GetAllAsync()
    {
        IReadOnlyList<Student> all =
            _store.Values.ToList();

        _logger.LogInformation(
            "Retrieved all students Count={Count}",
            all.Count);

        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);

        if (removed)
        {
            _logger.LogInformation(
                "Deleted student {StudentId}",
                id);
        }
        else
        {
            _logger.LogWarning(
                "Delete failed student {StudentId} not found",
                id);
        }

        return Task.FromResult(removed);
    }
}