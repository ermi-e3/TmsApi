namespace TmsApi.Application.DTOs;

public record StudentDetailDto
{
    public required int Id { get; init; }
    public required string RegistrationNumber { get; init; }
    public required string Name { get; init; }
    public required int Age { get; init; }
    public required decimal GPA { get; init; }
    public required bool IsActive { get; set; }
    public required int EnrollmentCount { get; init; }
    public required IReadOnlyList<LinkDto> Links { get; init; }
}
