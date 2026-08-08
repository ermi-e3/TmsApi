// using MediatR;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.RateLimiting;
// using TmsApi.Application.DTOs;
// using TmsApi.Application.Features.Courses.Queries.SearchCourses;
// using TmsApi.Application.Features.Transcripts.Commands.RequestTranscript;
// using TmsApi.Application.Features.Transcripts.Queries.GetTranscriptStatus;

// namespace TmsApi.Api.Controllers;

// [ApiController]
// [Route("api/v2/transcripts")]
// public class TranscriptsController(IMediator mediator) : ControllerBase
// {
//     [HttpPost]
//     [EnableRateLimiting("transcripts")]
//     public async Task<IActionResult> RequestTranscript(
//         [FromBody] RequestTranscriptRequest request,
//         CancellationToken ct
//     )
//     {
//         var jobId = await mediator.Send(new RequestTranscriptCommand(request.StudentId), ct);

//         var location = Url.Action(nameof(GetTranscriptStatus), "Transcripts", new { id = jobId });

//         return Accepted(location, new { id = jobId, status = "queued" });
//     }

//     [HttpGet("{id:guid}")]
//     public async Task<IActionResult> GetTranscriptStatus(Guid id, CancellationToken ct)
//     {
//         var result = await mediator.Send(new GetTranscriptStatusQuery(id), ct);

//         if (result is null)
//             return NotFound();

//         return Ok(result);
//     }

//     [HttpGet("search")]
//     [EnableRateLimiting("search")]
//     public async Task<IActionResult> SearchCourses([FromQuery] string? term, CancellationToken ct)
//     {
//         var results = await mediator.Send(new SearchCoursesQuery(term), ct);
//         return Ok(results);
//     }
// }

using System.Threading.Channels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
// [Route("api/v2/transcripts")]
[Route("api/v{version:apiVersion}/transcripts")]

public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore
) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript(
        TranscriptRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct
    )
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await statusStore.GetReportIdForIdempotencyKeyAsync(idempotencyKey, ct);
            if (existing is not null)
            {
                var existingStatus = await statusStore.GetAsync(existing, ct);
                return Accepted(
                    Url.Action(nameof(GetStatus), new { id = existing }),
                    existingStatus
                );
            }
        }

        var reportId = Guid.NewGuid().ToString("N")[..12];
        var status = await statusStore.CreateAsync(reportId, request.StudentId, ct);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            await statusStore.LinkIdempotencyKeyAsync(idempotencyKey, reportId, ct);
        await channel.Writer.WriteAsync(request.WithReportId(reportId), ct);

        Response.Headers.RetryAfter = "5";
        return Accepted(Url.Action(nameof(GetStatus), new { id = reportId }), status);
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(string id, CancellationToken ct)
    {
        var status = await statusStore.GetAsync(id, ct);
        return status is null
            ? NotFound(
                new ProblemDetails
                {
                    Title = "Transcript not found",
                    Detail = $"No transcript request with id '{id}'.",
                    Status = StatusCodes.Status404NotFound,
                }
            )
            : Ok(status);
    }
}
