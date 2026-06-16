namespace TmsApi.Models;

public record CreateCourseRequest(
    string Code,
    string Title,
    int Capacity);