using System.Collections.Concurrent;
using System.Threading.Channels;
using Claims.Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Infrastructure;

/// <summary>In-memory asynchronous audit transport. Replace with a durable broker for multi-instance production deployments.</summary>
public sealed class AuditQueue : BackgroundService, IAuditQueue
{
    private readonly Channel<AuditMessage> _channel = Channel.CreateUnbounded<AuditMessage>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    private readonly ConcurrentQueue<AuditMessage> _processed = new();
    private readonly ILogger<AuditQueue> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    public AuditQueue(ILogger<AuditQueue> logger, IServiceScopeFactory scopeFactory) { _logger = logger; _scopeFactory = scopeFactory; }
    public bool TryEnqueue(AuditMessage message) => _channel.Writer.TryWrite(message);
    public IReadOnlyCollection<AuditMessage> Processed => _processed.ToArray();
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
                db.AuditEntries.Add(new AuditEntry { Entity = message.Entity, EntityId = message.EntityId, Operation = message.Operation, OccurredAt = message.OccurredAt });
                await db.SaveChangesAsync(stoppingToken);
                _processed.Enqueue(message);
                _logger.LogInformation("Persisted audit {Operation} {Entity} {EntityId} at {OccurredAt}", message.Operation, message.Entity, message.EntityId, message.OccurredAt);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { _logger.LogError(exception, "Could not persist audit message for {Entity} {EntityId}", message.Entity, message.EntityId); }
        }
    }
    public override Task StopAsync(CancellationToken cancellationToken) { _channel.Writer.TryComplete(); return base.StopAsync(cancellationToken); }
}
