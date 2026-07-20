using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;

namespace Infrastructure.Outbox;

public class OutboxService : IOutboxService
{
    private readonly IOutboxRepository _outbox;

    public OutboxService(IOutboxRepository outbox) => _outbox = outbox;

    public void Publish<TMessage>(TMessage message) where TMessage : class
    {
        var entry = Domain.Entities.Outbox.OutboxMessage.Create(
            typeof(TMessage).AssemblyQualifiedName!,
            System.Text.Json.JsonSerializer.Serialize(message, message.GetType()));
        _outbox.AddAsync(entry).GetAwaiter().GetResult();
    }
}
