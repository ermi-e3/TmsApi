using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record UpdateStudentRequest
{
    [Required]
    [RegularExpression(
        @"^[A-Z]{3}-\d{4}-\d{4}$",
        ErrorMessage = "Code must follow the pattern XXX-0000-0000 (e.g., TMS-2026-0001)."
    )]
    public required string RegistrationNumber { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [Range(0.0, 4.0)]
    public decimal GPA { get; set; }

    [Range(16, 120)]
    public int Age { get; set; }
}
