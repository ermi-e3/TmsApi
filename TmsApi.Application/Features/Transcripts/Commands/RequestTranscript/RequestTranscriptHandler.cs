using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Application.TranscriptJobModel;

namespace TmsApi.Application.Features.Transcripts.Commands.RequestTranscript;

// public sealed class RequestTranscriptHandler(ITranscriptQueue transcriptQueue)
//     : IRequestHandler<RequestTranscriptCommand, Guid>
// {
//     public async Task<Guid> Handle(RequestTranscriptCommand command, CancellationToken ct)
//     {
//         var jobId = Guid.NewGuid();

//         var job = new TranscriptJob(jobId, command.StudentId, DateTimeOffset.UtcNow);

//         await transcriptQueue.EnqueueAsync(job, ct);

//         return jobId;
//     }
// }

// public sealed class RequestTranscriptHandler(ITranscriptQueue transcriptQueue)
//     : IRequestHandler<RequestTranscriptCommand, Guid>
// {
//     public async Task<Guid> Handle(RequestTranscriptCommand command, CancellationToken ct)
//     {
//         var jobId = Guid.NewGuid();

//         var job = new TranscriptJob
//         {
//             Id = jobId,
//             StudentId = command.StudentId,
//             CreatedAt = DateTimeOffset.UtcNow,
//             Status = TranscriptStatus.Queued,
//         };

//         await transcriptQueue.EnqueueAsync(job, ct);

//         return jobId;
//     }
// }

public sealed class RequestTranscriptHandler(
    ITranscriptQueue transcriptQueue,
    ITranscriptJobStore jobStore
) : IRequestHandler<RequestTranscriptCommand, Guid>
{
    public async Task<Guid> Handle(RequestTranscriptCommand command, CancellationToken ct)
    {
        var jobId = Guid.NewGuid();

        var job = new TranscriptJob
        {
            Id = jobId,
            StudentId = command.StudentId,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = TranscriptStatus.Queued,
        };

        await jobStore.AddAsync(job, ct);

        await transcriptQueue.EnqueueAsync(job, ct);

        return jobId;
    }
}
