namespace TmsApi.Models;

public record CreateStudentRequest(
    string Id,
    string Name,
    int Age,
    decimal GPA);