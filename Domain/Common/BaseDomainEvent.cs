namespace Domain.Common;

public abstract class BaseDomainEvent : IDomainEvent
{
    public DateTime OccurredOnUtc { get; }

    protected BaseDomainEvent()
    {
        OccurredOnUtc = DateTime.UtcNow;
    }
}