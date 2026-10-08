using Claims.Application.Abstractions;
using Claims.Application.Commands.Claims.CreateClaim;
using Claims.Application.Commands.Claims.DeleteClaim;
using Claims.Application.Commands.Covers.CreateCover;
using Claims.Application.Commands.Covers.DeleteCover;
using Claims.Application.Queries.Claims.GetClaim;
using Claims.Application.Queries.Claims.GetClaims;
using Claims.Application.Queries.Covers.CalculatePremium;
using Claims.Application.Queries.Covers.GetCover;
using Claims.Application.Queries.Covers.GetCovers;
using Claims.Domain;
using MediatR;

namespace Claims.Application.Auditing;

/// <summary>Queues an audit event for each successfully handled API use case without awaiting persistence.</summary>
public sealed class AuditingBehavior<TRequest, TResponse>(IAuditQueue auditQueue)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
        if (TryCreateMessage(request, response, out var message))
            auditQueue.TryEnqueue(message);

        return response;
    }

    private static bool TryCreateMessage(TRequest request, TResponse response, out AuditMessage message)
    {
        var now = DateTimeOffset.UtcNow;
        message = request switch
        {
            GetClaimsQuery => new("Claim", "*", "GET", now),
            GetClaimQuery query => new("Claim", response is Claim claim ? claim.Id.ToString() : $"displayId:{query.DisplayId}", "GET", now),
            CreateClaimCommand when response is Claim claim => new("Claim", claim.Id.ToString(), "POST", now),
            DeleteClaimCommand query => new("Claim", response is Claim claim ? claim.Id.ToString() : $"displayId:{query.DisplayId}", "DELETE", now),
            GetCoversQuery => new("Cover", "*", "GET", now),
            GetCoverQuery query => new("Cover", response is Cover cover ? cover.Id.ToString() : $"displayId:{query.DisplayId}", "GET", now),
            CreateCoverCommand when response is Cover cover => new("Cover", cover.Id.ToString(), "POST", now),
            DeleteCoverCommand query => new("Cover", response is Cover cover ? cover.Id.ToString() : $"displayId:{query.DisplayId}", "DELETE", now),
            CalculatePremiumQuery => new("Cover", "premium-calculation", "POST", now),
            _ => null!
        };

        return message is not null;
    }
}
