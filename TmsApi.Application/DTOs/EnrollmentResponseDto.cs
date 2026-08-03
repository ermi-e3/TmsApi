namespace TmsApi.Application.DTOs;

// public record EnrollmentResponseDto(
//     int Id,
//     int CourseId,
//     int StudentId,
//     DateTime EnrolledAt,
//     string Status
// );

public record EnrollmentResponseDto(
    int Id,
    int CourseId,
    string CourseTitle,
    int StudentId,
    string StudentName,
    DateTime EnrolledAt,
    string Status
);
