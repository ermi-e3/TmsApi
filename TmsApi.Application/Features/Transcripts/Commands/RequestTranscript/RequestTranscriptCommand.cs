using MediatR;

namespace TmsApi.Application.Features.Transcripts.Commands.RequestTranscript;

public sealed record RequestTranscriptCommand(int StudentId) : IRequest<Guid>;
