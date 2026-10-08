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

/// <summary>Creates, reads, deletes, and calculates premiums for insurance covers.</summary>
[ApiController, Route("[controller]")]
public sealed class CoversController(ISender sender) : ControllerBase
{
    /// <summary>Returns all covers.</summary>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <response code="200">The covers were returned.</response>
    [HttpGet, ProducesResponseType(typeof(IReadOnlyList<Cover>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<Cover>> Get(CancellationToken ct) => sender.Send(new GetCoversQuery(), ct);

    /// <summary>Finds a cover by its integer display ID.</summary>
    /// <param name="displayId">The cover's display ID.</param>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <response code="200">The cover was found.</response>
    /// <response code="400">The display ID must be positive.</response>
    /// <response code="404">No cover has this display ID.</response>
    [HttpGet("{displayId:int}"), ProducesResponseType(typeof(Cover), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Cover>> Get(int displayId, CancellationToken ct) => await sender.Send(new GetCoverQuery(displayId), ct) is { } cover ? Ok(cover) : NotFound();

    /// <summary>Creates a cover and calculates its premium.</summary>
    /// <param name="body">Cover dates and covered-object type.</param>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <response code="201">The cover was created.</response>
    /// <response code="400">The request violates cover validation rules.</response>
    [HttpPost, ProducesResponseType(typeof(Cover), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Cover>> Create(CreateCoverRequest body, CancellationToken ct)
    {
        var cover = await sender.Send(new CreateCoverCommand(body.StartDate, body.EndDate, body.Type), ct);
        return CreatedAtAction(nameof(Get), new { displayId = cover.DisplayId }, cover);
    }
    /// <summary>Calculates a premium without creating a cover.</summary>
    /// <param name="startDate">Coverage start date in YYYY-MM-DD format.</param>
    /// <param name="endDate">Coverage end date in YYYY-MM-DD format.</param>
    /// <param name="coverType">The covered-object type.</param>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <response code="200">The calculated premium.</response>
    /// <response code="400">The end date precedes the start date or a query value is invalid.</response>
    [HttpPost("compute"), ProducesResponseType(typeof(decimal), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<decimal> Compute([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] CoverType coverType, CancellationToken ct) => sender.Send(new CalculatePremiumQuery(startDate, endDate, coverType), ct);

    /// <summary>Deletes a cover by its integer display ID when no claims reference it.</summary>
    /// <param name="displayId">The cover's display ID.</param>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <response code="204">The cover was deleted.</response>
    /// <response code="400">The cover still has claims.</response>
    /// <response code="404">No cover has this display ID.</response>
    [HttpDelete("{displayId:int}"), ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int displayId, CancellationToken ct) => await sender.Send(new DeleteCoverCommand(displayId), ct) is not null ? NoContent() : NotFound();
}
