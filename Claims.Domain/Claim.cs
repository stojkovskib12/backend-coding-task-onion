namespace Claims.Domain;

/// <summary>An insurance claim linked to a cover.</summary>
/// <param name="Id">Stable GUID identifier.</param>
/// <param name="DisplayId">Database-generated integer identifier used in API routes and displays.</param>
/// <param name="CoverId">GUID identifier of the related cover.</param>
/// <param name="Created">Claim date, inclusive within the cover's effective dates.</param>
/// <param name="Name">Claim name.</param>
/// <param name="Type">Claim category.</param>
/// <param name="DamageCost">Claimed damage amount.</param>
public sealed record Claim(Guid Id, int DisplayId, Guid CoverId, DateOnly Created, string Name, ClaimType Type, decimal DamageCost);
