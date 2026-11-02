using Application.Common.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

public sealed class EmailDeliveryProcessor : BackgroundService
{
    private readonly IEmailDeliveryQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailDeliveryProcessor> _logger;

    public EmailDeliveryProcessor(
        IEmailDeliveryQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailDeliveryProcessor> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.ReadAllAsync(stoppingToken))
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var renderer = scope.ServiceProvider.GetRequiredService<EmailTemplateRenderer>();
                    var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var content = renderer.Render(job.Template, job.Model);

                    switch (job.RecipientKind)
                    {
                        case EmailRecipientKind.User:
                            await email.SendAsync(Guid.Parse(job.Recipient), content.Subject, content.HtmlBody, stoppingToken);
                            break;
                        case EmailRecipientKind.Address when job.Attachment is { Length: > 0 }:
                            await email.SendToEmailWithAttachmentAsync(job.Recipient, content.Subject, content.HtmlBody,
                                job.AttachmentName ?? "invoice.pdf", job.Attachment, stoppingToken);
                            break;
                        case EmailRecipientKind.Address:
                            await email.SendToEmailAsync(job.Recipient, content.Subject, content.HtmlBody, stoppingToken);
                            break;
                        case EmailRecipientKind.PlatformAdmins:
                            await email.SendToPlatformAdminsAsync(content.Subject, content.HtmlBody, stoppingToken);
                            break;
                        case EmailRecipientKind.SchoolAdmins:
                            await email.SendToSchoolAdminsAsync(Guid.Parse(job.Recipient), content.Subject, content.HtmlBody, stoppingToken);
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported email recipient kind: {job.RecipientKind}.");
                    }
                    break;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex) when (attempt < 3)
                {
                    _logger.LogWarning(ex, "Email template {Template} delivery attempt {Attempt} failed; retrying.",
                        job.Template, attempt);
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email template {Template} failed after {Attempt} attempts.",
                        job.Template, attempt);
                    break;
                }
            }
        }
    }
}
