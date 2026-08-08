using TmsApi.Application.DTOs;
using TmsApi.Application.TranscriptJobModel;

namespace TmsApi.Application.Interfaces;

public interface ITranscriptJobStore
{
    Task<TranscriptStatusResponse?> GetAsync(Guid id, CancellationToken ct);

    Task SetStatusAsync(Guid id, TranscriptStatus status, CancellationToken ct);

    Task AddAsync(TranscriptJob job, CancellationToken ct);
}
