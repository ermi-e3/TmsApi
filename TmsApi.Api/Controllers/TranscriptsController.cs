using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.DTOs;
using TmsApi.Application.Features.Courses.Queries.SearchCourses;
using TmsApi.Application.Features.Transcripts.Commands.RequestTranscript;
using TmsApi.Application.Features.Transcripts.Queries.GetTranscriptStatus;

namespace TmsApi.Api.Controllers;


[ApiController]
[Route("api/v2/transcripts")]
public class TranscriptsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript(
        [FromBody] RequestTranscriptRequest request,
        CancellationToken ct
    )
    {
        var jobId = await mediator.Send(new RequestTranscriptCommand(request.StudentId), ct);

        var location = Url.Action(nameof(GetTranscriptStatus), "Transcripts", new { id = jobId });

        return Accepted(location, new { id = jobId, status = "queued" });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTranscriptStatus(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTranscriptStatusQuery(id), ct);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<IActionResult> SearchCourses([FromQuery] string? term, CancellationToken ct)
    {
        var results = await mediator.Send(new SearchCoursesQuery(term), ct);
        return Ok(results);
    }
}
