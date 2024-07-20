using MediatR;
using MediatR.Pipeline;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Commands;

public class BasePreProcessor<TRequest, TResponse> : IRequestPreProcessor<TRequest>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Represents the context for interacting with the Stargate database.</summary>
    protected readonly StargateContext _context;

    /// <summary>Initializes a new instance of the BasePreProcessor class.</summary>
    /// <param name="context">The StargateContext used for creating an object.</param>
    public BasePreProcessor(StargateContext context)
    {
        _context = context;
    }

    public virtual Task Process(TRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}