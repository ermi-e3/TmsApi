namespace TmsApi.Application.DTOs;

public record GradeRequest(int StudentId, int CourseId, decimal Score);
