using System.Diagnostics;
using Application.Common.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

/// <summary>
/// A MediatR pipeline behavior that logs the execution time of requests and handles exceptions.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IEmailNotifier _emailNotifier;
    private const int SlowRequestThresholdMs = 500;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IEmailNotifier emailNotifier)
    {
        _logger = logger;
        _emailNotifier = emailNotifier;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("[START] {RequestName}", requestName);
            var response = await next();
            // All transaction behaviors are inside this outer behavior, so email
            // only leaves the process after command persistence succeeds.
            await _emailNotifier.FlushAsync(cancellationToken);
            sw.Stop();

            if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
                _logger.LogWarning("[SLOW] {RequestName} took {Elapsed}ms — check for missing indexes or N+1 queries.", requestName, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation("[END] {RequestName} completed in {Elapsed}ms", requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[FAIL] {RequestName} failed after {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
