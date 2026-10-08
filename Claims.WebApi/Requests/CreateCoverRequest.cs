using Claims.Domain;

namespace Claims.WebApi.Requests;

/// <summary>Payload used to issue an insurance cover.</summary>
/// <param name="StartDate">Coverage start date; must be today or later.</param>
/// <param name="EndDate">Coverage end date; must be later than start and no more than one year after it.</param>
/// <param name="Type">Type of covered vessel.</param>
public sealed record CreateCoverRequest(DateOnly StartDate, DateOnly EndDate, CoverType Type);
