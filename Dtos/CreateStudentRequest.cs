using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record CreateStudentRequest
{
    // TMS-2026-0001

    // [ Required, RegularExpression(  @"^[A-Z]{3}-\d{3}$", ErrorMessage = "Code must follow the pattern XXX-000 (e.g., CSE-101).")]
    [
        Required,
        RegularExpression(
            @"^[A-Z]{3}-\d{4}-\d{4}$",
            ErrorMessage = "Code must follow the pattern XXX-0000-0000 (e.g., TMS-2026-0001)."
        )
    ]
    public required string RegistrationNumber { get; set; }

    [Required, MaxLength(200)]
    public required string Name { get; set; }

    [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0.")]
    public decimal GPA { get; set; }

    [Range(16, 120, ErrorMessage = "Age must be between 16 and 120.")]
    public int Age { get; set; }
}


//   public required string RegistrationNumber { get; set; } // natural key — human-readable (uniqueness configured in Session 2)
//     public required string Name { get; set; }
//     public decimal GPA { get; set; }
//     public int Age { get; set; }
