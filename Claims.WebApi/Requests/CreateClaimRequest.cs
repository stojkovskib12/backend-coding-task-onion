using Claims.Domain;

namespace Claims.WebApi.Requests;

public sealed record CreateClaimRequest(string CoverId, DateOnly Created, string Name, ClaimType Type, decimal DamageCost);
