using TmsApi.Models;

namespace TmsApi.Entities;

public class CourseService : ICourseService
{
    private readonly Dictionary<string, Course> _store = new();

    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public Task<Course> CreateAsync(
        int id,
        string code,
        string title,
        int capacity,
        ICollection<Enrollment> enrollments
    )
    {
        _logger.LogInformation("Course creation request received for {CourseCode}", code);

        _logger.LogInformation("Current course count: {Count}", _store.Count);

        if (_store.TryGetValue(code, out var existing))
        {
            _logger.LogWarning("Duplicate course creation attempt {CourseCode}", code);

            return Task.FromResult(existing);
        }

        var course = new Course
        {
            Id = id,
            Code = code,
            Title = title,
            Capacity = capacity,
            Enrollments = enrollments,
        };

        _store[code] = course;

        _logger.LogInformation("Created course {CourseCode}", code);

        _logger.LogInformation("Course count after insert: {Count}", _store.Count);

        return Task.FromResult(course);
    }

    public Task<Course?> GetByIdAsync(string code)
    {
        _store.TryGetValue(code, out var course);

        if (course is null)
        {
            _logger.LogWarning("Course {CourseCode} not found", code);
        }

        return Task.FromResult(course);
    }

    public Task<IReadOnlyList<Course>> GetAllAsync()
    {
        IReadOnlyList<Course> all = _store.Values.ToList();

        _logger.LogInformation("Retrieved all courses Count={Count}", all.Count);

        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string code)
    {
        var removed = _store.Remove(code);

        if (removed)
        {
            _logger.LogInformation("Deleted course {CourseCode}", code);
        }
        else
        {
            _logger.LogWarning("Delete failed course {CourseCode} not found", code);
        }

        return Task.FromResult(removed);
    }
}
