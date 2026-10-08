using Claims.Application.Commands.Claims.CreateClaim;
using Claims.Application.Commands.Claims.DeleteClaim;
using Claims.Application.Queries.Claims.GetClaim;
using Claims.Application.Queries.Claims.GetClaims;
using Claims.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Claims.WebApi.Requests;

namespace Claims.WebApi.Controllers;

/// <summary>Creates, reads, and deletes insurance claims.</summary>
[ApiController, Route("[controller]")]
public sealed class ClaimsController(ISender sender) : ControllerBase
{
    /// <summary>Returns all claims.</summary>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <response code="200">The claims were returned.</response>
    [HttpGet, ProducesResponseType(typeof(IReadOnlyList<Claim>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<Claim>> Get(CancellationToken ct) => sender.Send(new GetClaimsQuery(), ct);

    /// <summary>Finds a claim by its integer display ID.</summary>
    /// <param name="displayId">The claim's display ID.</param>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <response code="200">The claim was found.</response>
    /// <response code="400">The display ID must be positive.</response>
    /// <response code="404">No claim has this display ID.</response>
    [HttpGet("{displayId:int}"), ProducesResponseType(typeof(Claim), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Claim>> Get(int displayId, CancellationToken ct) => await sender.Send(new GetClaimQuery(displayId), ct) is { } claim ? Ok(claim) : NotFound();

    /// <summary>Creates a claim for an existing cover.</summary>
    /// <param name="body">Claim data. <c>CoverId</c> is the cover's GUID identifier.</param>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <response code="201">The claim was created.</response>
    /// <response code="400">The request violates claim validation rules.</response>
    [HttpPost, ProducesResponseType(typeof(Claim), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Claim>> Create(CreateClaimRequest body, CancellationToken ct)
    {
        var claim = await sender.Send(new CreateClaimCommand(body.CoverId, body.Created, body.Name, body.Type, body.DamageCost), ct);
        return CreatedAtAction(nameof(Get), new { displayId = claim.DisplayId }, claim);
    }
    /// <summary>Deletes a claim by its integer display ID.</summary>
    /// <param name="displayId">The claim's display ID.</param>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <response code="204">The claim was deleted.</response>
    /// <response code="400">The display ID must be positive.</response>
    /// <response code="404">No claim has this display ID.</response>
    [HttpDelete("{displayId:int}"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int displayId, CancellationToken ct) => await sender.Send(new DeleteClaimCommand(displayId), ct) is not null ? NoContent() : NotFound();
}
