using System.Collections.Concurrent;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.TranscriptJobModel;

public sealed class TranscriptJobStore : ITranscriptJobStore
{
    // private readonly ConcurrentDictionary<Guid, TranscriptStatus> jobs = new();

    // public void Add(Guid id)
    // {
    //     jobs[id] = TranscriptStatus.Queued;
    // }

    // public Task<TranscriptStatusResponse?> GetAsync(
    //     Guid id,
    //     CancellationToken ct)
    // {
    //     if (!jobs.TryGetValue(id, out var status))
    //         return Task.FromResult<TranscriptStatusResponse?>(null);

    //     return Task.FromResult<TranscriptStatusResponse?>(
    //         new TranscriptStatusResponse(id, status.ToString()));
    // }

    // public Task SetStatusAsync(
    //     Guid id,
    //     TranscriptStatus status,
    //     CancellationToken ct)
    // {
    //     jobs[id] = status;

    //     return Task.CompletedTask;
    // }
    private readonly ConcurrentDictionary<Guid, TranscriptJob> jobs = new();

    public Task AddAsync(TranscriptJob job, CancellationToken ct)
    {
        jobs[job.Id] = job;

        return Task.CompletedTask;
    }

    public Task<TranscriptStatusResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        if (!jobs.TryGetValue(id, out var job))
        {
            return Task.FromResult<TranscriptStatusResponse?>(null);
        }

        var response = new TranscriptStatusResponse(job.Id, job.Status.ToString());

        return Task.FromResult<TranscriptStatusResponse?>(response);
    }

    public Task SetStatusAsync(Guid id, TranscriptStatus status, CancellationToken ct)
    {
        if (jobs.TryGetValue(id, out var job))
        {
            job.Status = status;
        }

        return Task.CompletedTask;
    }
}
