using MediatR;
using System.Transactions;

namespace Core.Application.Pipelines.Transaction;

public class TransactionScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ITransactionalRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using TransactionScope transactionScope = new TransactionScope(asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);
        {
            TResponse response;
            try
            {
                response = await next();
                transactionScope.Complete();
            }
            catch (Exception ex)
            {

                transactionScope.Dispose();
                throw new Exception($"An error occurred while processing the transaction. {ex.Message}", ex);
            }
            return response;
        }
    }
}
