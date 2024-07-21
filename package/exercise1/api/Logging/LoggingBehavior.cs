using MediatR;
using System.Diagnostics;
using StargateAPI.Repositories;

namespace StargateAPI.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILoggingRepository<TRequest, TResponse> _repository;

    public LoggingBehavior(
        ILoggingRepository<TRequest, TResponse> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string responseName = typeof(TResponse).Name;
        string guidValue = Guid.NewGuid().ToString();

        /////
        // Pre-processing
        /////

        await _repository.LogRequestStartAsync(request, guidValue, cancellationToken);
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            await _repository.LogRequestPropsAsync(request, guidValue, cancellationToken);
        }
        catch (NotSupportedException)
        {
            await _repository.LogRequestPropSerializationErrorAsync(request, guidValue, cancellationToken);
        }

        TResponse? response = await next();

        /////
        // Post-processing
        /////

        stopwatch.Stop();
        await _repository.LogRequestEndAsync(response, guidValue, stopwatch, cancellationToken);

        return response;
    }
}