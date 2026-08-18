namespace TmsApi.Domain.Entities;

public class Grade
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;

    public decimal Score { get; set; }

    public DateTime GradedAt { get; set; } = DateTime.UtcNow;
}