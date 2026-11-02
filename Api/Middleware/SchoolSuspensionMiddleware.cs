using System.Text.Json;
using Application.Common.Constants;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Middleware;

/// <summary>
/// Blocks every request from users whose school was suspended (or deleted) by a
/// platform administrator. This covers existing sessions: a user who signed in
/// before the suspension is rejected on the next request.
///
/// Authentication endpoints stay reachable so a user can sign in and read the
/// suspension message, or switch to another school they still belong to.
/// Platform administrators are never blocked.
/// </summary>
public sealed class SchoolSuspensionMiddleware
{
    private static readonly string[] ExemptPrefixes =
    [
        "/api/v1/auth",
        "/openapi",
        "/scalar",
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<SchoolSuspensionMiddleware> _logger;

    public SchoolSuspensionMiddleware(
        RequestDelegate next,
        ILogger<SchoolSuspensionMiddleware> logger)
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

        var school = await platformDb.Schools
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == schoolId, context.RequestAborted);

        if (school is not null && school.IsActive)
        {
            await _next(context);
            return;
        }

        _logger.LogInformation(
            "Blocked {Method} {Path} for school {SchoolId}: school is {Status}.",
            context.Request.Method, context.Request.Path, schoolId,
            school is null ? "missing" : school.Status);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = StatusCodes.Status403Forbidden,
            code = "school_suspended",
            message = SchoolAccessMessages.Suspended,
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
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
