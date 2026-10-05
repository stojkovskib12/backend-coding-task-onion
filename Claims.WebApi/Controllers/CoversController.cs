using Claims.Application.Commands.Covers.CreateCover;
using Claims.Application.Commands.Covers.DeleteCover;
using Claims.Application.Queries.Covers.CalculatePremium;
using Claims.Application.Queries.Covers.GetCover;
using Claims.Application.Queries.Covers.GetCovers;
using Claims.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Claims.WebApi.Requests;

namespace Claims.WebApi.Controllers;

[ApiController, Route("[controller]")]
public sealed class CoversController(ISender sender) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyList<Cover>> Get(CancellationToken ct) => sender.Send(new GetCoversQuery(), ct);
    [HttpGet("{id}")] public async Task<ActionResult<Cover>> Get(string id, CancellationToken ct) => await sender.Send(new GetCoverQuery(id), ct) is { } cover ? Ok(cover) : NotFound();
    [HttpPost] public async Task<ActionResult<Cover>> Create(CreateCoverRequest body, CancellationToken ct)
    {
        var cover = await sender.Send(new CreateCoverCommand(body.StartDate, body.EndDate, body.Type), ct);
        return CreatedAtAction(nameof(Get), new { id = cover.Id }, cover);
    }
    [HttpPost("compute")] public Task<decimal> Compute([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] CoverType coverType, CancellationToken ct) => sender.Send(new CalculatePremiumQuery(startDate, endDate, coverType), ct);
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(string id, CancellationToken ct) => await sender.Send(new DeleteCoverCommand(id), ct) ? NoContent() : NotFound();
}
