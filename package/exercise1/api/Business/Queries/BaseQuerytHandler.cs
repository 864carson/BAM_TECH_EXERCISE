using MediatR;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Queries;

public class BaseQueryHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Represents the context for interacting with the Stargate database.</summary>
    protected readonly StargateContext _context;

    /// <summary>Initializes a new instance of the BaseQueryHandler class.</summary>
    /// <param name="context">The StargateContext used for retrieving a object.</param>
    public BaseQueryHandler(StargateContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles a request asynchronously.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    /// <exception cref="NotImplementedException">Thrown when the method is not implemented.</exception>
    public virtual Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}