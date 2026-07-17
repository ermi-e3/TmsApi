using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record PatchStudentRequest
{
    [RegularExpression(
        @"^[A-Z]{3}-\d{4}-\d{4}$",
        ErrorMessage = "Code must follow the pattern XXX-0000-0000 (e.g., TMS-2026-0001)."
    )]
    public string? RegistrationNumber { get; set; }
    public string? Name { get; init; }

    public decimal? GPA { get; init; }

    public int? Age { get; init; }
}