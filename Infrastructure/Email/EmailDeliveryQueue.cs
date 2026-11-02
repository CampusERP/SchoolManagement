using System.Threading.Channels;
using Application.Common.Interfaces.Services;

namespace Infrastructure.Email;

public sealed class EmailDeliveryQueue : IEmailDeliveryQueue
{
    private readonly Channel<EmailDeliveryJob> _channel = Channel.CreateUnbounded<EmailDeliveryJob>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public ValueTask EnqueueAsync(EmailDeliveryJob job, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(job, ct);

    public IAsyncEnumerable<EmailDeliveryJob> ReadAllAsync(CancellationToken ct = default) =>
        _channel.Reader.ReadAllAsync(ct);
}
