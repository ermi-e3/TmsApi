namespace TmsApi.Application.DTOs;

public record StudentResponseDto(
    int Id,
    string RegistrationNumber,
    string Name,
    int Age,
    decimal GPA,
    bool IsActive,
    int EnrollmentCount
);
