namespace TmsApi.Application.TranscriptJobModel;

public sealed class TranscriptJob
{
    public Guid Id { get; set; }
    public int StudentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public TranscriptStatus Status { get; set; }
}
