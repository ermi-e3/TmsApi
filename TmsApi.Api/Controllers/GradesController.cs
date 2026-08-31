using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Features.Grades.Commands;


namespace TmsApi.Api.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
// [Route("api/v2/grades")]
[Route("api/v{version:apiVersion}/grades")]

public class GradesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitGrade(SubmitGradeCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        return Ok(new { id = result.Id.ToString(), success = result.Success });
    }
}
