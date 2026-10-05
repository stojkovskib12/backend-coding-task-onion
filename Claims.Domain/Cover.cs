namespace Claims.Domain;

public sealed record Cover(string Id, DateOnly StartDate, DateOnly EndDate, CoverType Type, decimal Premium);
