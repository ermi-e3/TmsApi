using System.Threading.Channels;

namespace TmsApi.Application.TranscriptJobModel;

public sealed class TranscriptQueue : ITranscriptQueue
{
    private readonly Channel<TranscriptJob> channel = Channel.CreateUnbounded<TranscriptJob>();

    public async ValueTask EnqueueAsync(TranscriptJob job, CancellationToken ct)
    {
        await channel.Writer.WriteAsync(job, ct);
    }

    public async ValueTask<TranscriptJob> DequeueAsync(CancellationToken ct)
    {
        return await channel.Reader.ReadAsync(ct);
    }
}
