using Claims.Domain;

namespace Claims.WebApi.Requests;

/// <summary>Payload used to report an insurance claim.</summary>
/// <param name="CoverId">GUID identifier of an existing cover.</param>
/// <param name="Created">Date of the claim, which must be inside the cover period.</param>
/// <param name="Name">Required claim name, at most 200 characters.</param>
/// <param name="Type">Claim category.</param>
/// <param name="DamageCost">Damage amount from 0 through 100,000 inclusive.</param>
public sealed record CreateClaimRequest(Guid CoverId, DateOnly Created, string Name, ClaimType Type, decimal DamageCost);
