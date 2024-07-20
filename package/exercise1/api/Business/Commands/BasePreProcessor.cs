using MediatR;
using MediatR.Pipeline;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Commands;

public abstract class BasePreProcessor<TRequest, TResponse> : IRequestPreProcessor<TRequest>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Represents the logic-specific part of the application configuration.</summary>
    protected readonly AppConfig _config;

    /// <summary>Represents the context for interacting with the Stargate database.</summary>
    protected readonly StargateContext _context;

    /// <summary>Initializes a new instance of the BasePreProcessor class.</summary>
    /// <param name="context">The StargateContext used for creating an object.</param>
    public BasePreProcessor(AppConfig config, StargateContext context)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Processes a request asynchronously.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    /// <exception cref="NotImplementedException">Thrown to indicate that the method is not implemented.</exception>
    public abstract Task Process(TRequest request, CancellationToken cancellationToken);
}