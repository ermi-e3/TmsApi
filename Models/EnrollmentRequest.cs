namespace TmsApi.Models;

public record EnrollmentRequest(
    string StudentId,
    string CourseCode);