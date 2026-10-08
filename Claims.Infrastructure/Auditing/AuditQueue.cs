using System.Threading.Channels;
using Claims.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Claims.Infrastructure.Auditing;

/// <summary>Accepts audit messages quickly and persists them on a hosted background worker.</summary>
public sealed class AuditQueue : BackgroundService, IAuditQueue
{
    private const int Capacity = 2_000;
    private readonly Channel<AuditMessage> _channel = Channel.CreateBounded<AuditMessage>(new BoundedChannelOptions(Capacity)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait
    });

    private readonly ILogger<AuditQueue> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AuditQueue(ILogger<AuditQueue> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <summary>Queues without waiting for SQL. Returns false if the queue is full or stopping.</summary>
    public bool TryEnqueue(AuditMessage message)
    {
        var accepted = _channel.Writer.TryWrite(message);
        if (!accepted)
            _logger.LogWarning("Audit queue is full or stopping; dropped {Operation} for {Entity} {EntityId}", message.Operation, message.Entity, message.EntityId);

        return accepted;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AuditContext>();
                switch (message.Entity)
                {
                    case "Claim":
                        db.ClaimAudits.Add(new ClaimAudit
                        {
                            ClaimId = message.EntityId,
                            Created = message.OccurredAt.UtcDateTime,
                            HttpRequestType = message.Operation
                        });
                        break;
                    case "Cover":
                        db.CoverAudits.Add(new CoverAudit
                        {
                            CoverId = message.EntityId,
                            Created = message.OccurredAt.UtcDateTime,
                            HttpRequestType = message.Operation
                        });
                        break;
                    default:
                        _logger.LogWarning("Ignoring audit message for unknown entity {Entity}", message.Entity);
                        continue;
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not persist audit message for {Entity} {EntityId}", message.Entity, message.EntityId);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();
        return base.StopAsync(cancellationToken);
    }
}
