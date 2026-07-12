using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Behaviors;

/// <summary>
/// after every command. If the command didn't touch PlatformDbContext,
/// EF Core detects zero tracked changes and skips the round-trip entirely.
/// </summary>
public class PlatformTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IPlatformUnitOfWork _platformUnitOfWork;

    public PlatformTransactionBehavior(IPlatformUnitOfWork platformUnitOfWork)
    {
        _platformUnitOfWork = platformUnitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is ICommand or IBaseCommand)
            await _platformUnitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}