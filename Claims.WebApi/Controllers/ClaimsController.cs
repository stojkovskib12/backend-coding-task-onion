using Claims.Application.Commands.Claims.CreateClaim;
using Claims.Application.Commands.Claims.DeleteClaim;
using Claims.Application.Queries.Claims.GetClaim;
using Claims.Application.Queries.Claims.GetClaims;
using Claims.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Claims.WebApi.Requests;

namespace Claims.WebApi.Controllers;

[ApiController, Route("[controller]")]
public sealed class ClaimsController(ISender sender) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyList<Claim>> Get(CancellationToken ct) => sender.Send(new GetClaimsQuery(), ct);
    [HttpGet("{id}")] public async Task<ActionResult<Claim>> Get(string id, CancellationToken ct) => await sender.Send(new GetClaimQuery(id), ct) is { } claim ? Ok(claim) : NotFound();
    [HttpPost] public async Task<ActionResult<Claim>> Create(CreateClaimRequest body, CancellationToken ct)
    {
        var claim = await sender.Send(new CreateClaimCommand(body.CoverId, body.Created, body.Name, body.Type, body.DamageCost), ct);
        return CreatedAtAction(nameof(Get), new { id = claim.Id }, claim);
    }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(string id, CancellationToken ct) => await sender.Send(new DeleteClaimCommand(id), ct) ? NoContent() : NotFound();
}
