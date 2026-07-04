using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Behaviors;

/// <summary>
/// A MediatR pipeline behavior that wraps command handlers in a transaction.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        // Only save for commands — queries do not write.
        if (request is ICommand or IBaseCommand)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}

// Marker interfaces — every MediatR request used as a command implements one.
public interface ICommand : IRequest<Models.Result> { }
public interface ICommand<TResponse> : IRequest<Models.Result<TResponse>> { }
public interface IBaseCommand { } // non-generic fallback for TransactionBehavior check
