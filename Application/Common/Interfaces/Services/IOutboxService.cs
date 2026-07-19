namespace Application.Common.Interfaces.Services;

public interface IOutboxService
{
    void Publish<TMessage>(TMessage message) where TMessage : class;
}

public interface IOutboxMessageHandler<TMessage> where TMessage : class
{
    Task HandleAsync(TMessage message, CancellationToken ct);
}