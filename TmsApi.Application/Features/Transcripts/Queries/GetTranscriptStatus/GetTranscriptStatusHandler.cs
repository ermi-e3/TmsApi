using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Transcripts.Queries.GetTranscriptStatus;

public sealed class GetTranscriptStatusHandler(
    ITranscriptJobStore jobStore)
    : IRequestHandler<GetTranscriptStatusQuery, TranscriptStatusResponse?>
{
    public async Task<TranscriptStatusResponse?> Handle(
        GetTranscriptStatusQuery query,
        CancellationToken ct)
    {
        return await jobStore.GetAsync(query.Id, ct);
    }
}