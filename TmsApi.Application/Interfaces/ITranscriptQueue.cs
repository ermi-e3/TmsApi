using System.Threading.Channels;
using TmsApi.Application.TranscriptJobModel;

public interface ITranscriptQueue
{
    ValueTask EnqueueAsync(TranscriptJob job, CancellationToken ct);

    ValueTask<TranscriptJob> DequeueAsync(CancellationToken ct);
}
