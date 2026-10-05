namespace Claims.Application.Abstractions;

public interface IAuditQueue
{
    bool TryEnqueue(AuditMessage message);
}
