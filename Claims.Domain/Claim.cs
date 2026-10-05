namespace Claims.Domain;

public sealed record Claim(string Id, string CoverId, DateOnly Created, string Name, ClaimType Type, decimal DamageCost);
