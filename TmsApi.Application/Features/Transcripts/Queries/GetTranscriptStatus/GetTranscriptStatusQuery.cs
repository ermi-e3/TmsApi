using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Features.Transcripts.Queries.GetTranscriptStatus;

public sealed record GetTranscriptStatusQuery(Guid Id) : IRequest<TranscriptStatusResponse?>;
