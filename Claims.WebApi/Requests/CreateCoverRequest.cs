using Claims.Domain;

namespace Claims.WebApi.Requests;

public sealed record CreateCoverRequest(DateOnly StartDate, DateOnly EndDate, CoverType Type);
