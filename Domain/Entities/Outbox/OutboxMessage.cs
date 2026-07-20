using Domain.Common;

namespace Domain.Entities.Outbox;

public class OutboxMessage : Entity
{
    public string    Type           { get; private set; } = default!;
    public string    Payload        { get; private set; } = default!;
    public DateTime  CreatedAtUtc   { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public string?   Error          { get; private set; }
    public int       RetryCount     { get; private set; }

    public const int MaxRetries = 5;

    public bool IsProcessed  => ProcessedAtUtc is not null;
    public bool CanRetry     => RetryCount < MaxRetries;

    private OutboxMessage() { }

    private OutboxMessage(Guid id, string type, string payload) : base(id)
    {
        Type         = type;
        Payload      = payload;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static OutboxMessage Create(string type, string payload)
        => new(Guid.NewGuid(), type, payload);

    public void MarkProcessed()
    {
        ProcessedAtUtc = DateTime.UtcNow;
        Error          = null;
    }

    public void MarkFailed(string error)
    {
        Error      = error;
        RetryCount++;
    }
}
