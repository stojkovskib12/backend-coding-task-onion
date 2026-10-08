namespace Claims.Domain;

/// <summary>An insurance cover and its calculated premium.</summary>
/// <param name="Id">Stable GUID identifier used by related claims.</param>
/// <param name="DisplayId">Database-generated integer identifier used in API routes and displays.</param>
/// <param name="StartDate">First effective coverage date.</param>
/// <param name="EndDate">Coverage end date.</param>
/// <param name="Type">Type of vessel covered.</param>
/// <param name="Premium">Calculated premium for the elapsed coverage period.</param>
public sealed record Cover(Guid Id, int DisplayId, DateOnly StartDate, DateOnly EndDate, CoverType Type, decimal Premium);
