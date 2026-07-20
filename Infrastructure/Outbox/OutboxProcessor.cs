using Application.Common.Interfaces.Services;
using Application.Common.Messages;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Outbox;

/// <summary>Dispatches persisted integration messages without coupling Application to delivery infrastructure.</summary>
public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Outbox batch processing failed."); }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                { logger.LogError(ex, "Outbox batch processing failed."); }
                break;
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var messages = await db.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null && x.RetryCount < Domain.Entities.Outbox.OutboxMessage.MaxRetries)
            .OrderBy(x => x.CreatedAtUtc).Take(25).ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                await DispatchAsync(scope.ServiceProvider, message.Type, message.Payload, ct);
                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox message {MessageId} of type {Type} failed.", message.Id, message.Type);
                message.MarkFailed(ex.Message);
            }
        }

        if (messages.Count > 0) await db.SaveChangesAsync(ct);
    }

    private static async Task DispatchAsync(IServiceProvider services, string type, string payload, CancellationToken ct)
    {
        switch (type)
        {
            case nameof(CreateSchoolAdminProfileMessage):
                await HandleAsync<CreateSchoolAdminProfileMessage>(services, payload, ct); break;
            case nameof(LinkStudentLoginMessage):
                await HandleAsync<LinkStudentLoginMessage>(services, payload, ct); break;
            case nameof(LinkTeacherLoginMessage):
                await HandleAsync<LinkTeacherLoginMessage>(services, payload, ct); break;
            case nameof(LinkParentLoginMessage):
                await HandleAsync<LinkParentLoginMessage>(services, payload, ct); break;
            case nameof(DeliverNotificationBatchMessage):
                await HandleAsync<DeliverNotificationBatchMessage>(services, payload, ct); break;
            default: throw new InvalidOperationException($"Unsupported outbox message type '{type}'.");
        }
    }

    private static async Task HandleAsync<TMessage>(IServiceProvider services, string payload, CancellationToken ct) where TMessage : class
    {
        var message = JsonSerializer.Deserialize<TMessage>(payload)
            ?? throw new InvalidOperationException($"Outbox payload for {typeof(TMessage).Name} is invalid.");
        await services.GetRequiredService<IOutboxMessageHandler<TMessage>>().HandleAsync(message, ct);
    }
}
