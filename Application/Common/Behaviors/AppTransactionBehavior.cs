using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Behaviors;

/// <summary>
/// Saves ApplicationDbContext (tenant-scoped data)
/// Queries are skipped — they never write.
/// </summary>
public class AppTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public AppTransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is ICommand or IBaseCommand)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}

// Marker interfaces — every MediatR request used as a command implements one.
public interface ICommand : IRequest<Models.Result> { }
public interface ICommand<TResponse> : IRequest<Models.Result<TResponse>> { }
public interface IBaseCommand { } // non-generic fallback