using System.Text.Json;
using System.Text.Json.Nodes;
using Domain.Entities.Billing;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Middleware;

/// <summary>
/// Blocks school users from using the platform when their subscription is not in a
/// paid, running period. Billing, authentication, and registration endpoints stay
/// reachable so a school can pay an invoice or contact the platform.
///
/// School suspension itself is enforced earlier by <see cref="SchoolSuspensionMiddleware"/>,
/// which locks every account in a suspended school out of the system.
/// </summary>
public sealed class SubscriptionEnforcementMiddleware
{
    private static readonly string[] ExemptPrefixes =
    [
        "/api/v1/auth",
        "/api/v1/billing",
        "/api/v1/schoolregistrationrequests",
        "/api/v1/education-stages",
        "/openapi",
        "/scalar",
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<SubscriptionEnforcementMiddleware> _logger;

    public SubscriptionEnforcementMiddleware(
        RequestDelegate next,
        ILogger<SubscriptionEnforcementMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, PlatformDbContext platformDb)
    {
        if (!ShouldEnforce(context))
        {
            await _next(context);
            return;
        }

        var schoolIdValue = context.User.FindFirst("school_id")?.Value;
        if (!Guid.TryParse(schoolIdValue, out var schoolId))
        {
            await _next(context);
            return;
        }

        var subscription = await platformDb.Subscriptions
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync(context.RequestAborted);

        // Legacy schools without any subscription keep working; billing is opt-in
        // until the platform assigns them a plan.
        if (subscription is null)
        {
            await _next(context);
            return;
        }

        var now = DateTime.UtcNow;
        var isBlocked = subscription.Status switch
        {
            SubscriptionStatus.Active => now > subscription.EndDate,
            SubscriptionStatus.PendingPayment => true,
            SubscriptionStatus.Suspended => true,
            SubscriptionStatus.Cancelled => true,
            SubscriptionStatus.Expired => true,
            _ => true,
        };

        if (!isBlocked)
        {
            await _next(context);
            return;
        }

        _logger.LogInformation(
            "Blocked {Method} {Path} for school {SchoolId}: subscription {Status} ended {EndDate:u}.",
            context.Request.Method, context.Request.Path, schoolId,
            subscription.Status, subscription.EndDate);

        var message = subscription.Status switch
        {
            SubscriptionStatus.PendingPayment =>
                "Your subscription is awaiting payment. Please settle the invoice to activate your account.",
            SubscriptionStatus.Suspended =>
                "Your subscription is suspended. Please contact the platform to restore access.",
            SubscriptionStatus.Cancelled =>
                "Your subscription was cancelled. Please contact the platform to reactivate it.",
            _ =>
                "Your subscription period has ended. Please renew or contact the platform to continue.",
        };

        context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var payload = new JsonObject
        {
            ["status"] = StatusCodes.Status402PaymentRequired,
            ["code"] = "subscription_required",
            ["message"] = message,
            ["subscriptionStatus"] = subscription.Status.ToString(),
            ["subscriptionEndDate"] = JsonSerializer.SerializeToNode(subscription.EndDate, options),
        };

        await context.Response.WriteAsync(payload.ToJsonString());
    }

    private static bool ShouldEnforce(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return false;

        if (context.User.FindFirst("is_platform_admin")?.Value == "true")
            return false;

        var path = context.Request.Path.Value ?? string.Empty;
        return !ExemptPrefixes.Any(prefix =>
            path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
