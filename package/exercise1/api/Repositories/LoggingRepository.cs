using System.Diagnostics;
using System.Text.Json;
using StargateAPI.Business.Data;

namespace StargateAPI.Repositories;

public interface ILoggingRepository<TRequest, TResponse>
{
    Task LogRequestStartAsync(
        TRequest request,
        string identifier,
        CancellationToken cancellationToken = default);

    Task LogRequestPropsAsync(
        TRequest request,
        string identifier,
        CancellationToken cancellationToken = default);

    Task LogRequestPropSerializationErrorAsync(
        TRequest request,
        string identifier,
        CancellationToken cancellationToken = default);

    Task LogRequestEndAsync(
        TResponse response,
        string identifier,
        Stopwatch stopwatch,
        CancellationToken cancellationToken = default);
}

public class LoggingRepository<TRequest, TResponse> : ILoggingRepository<TRequest, TResponse>
{
    /// <summary>Represents the context for interacting with the Stargate database.</summary>
    private readonly StargateContext _context;

    public LoggingRepository(StargateContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task LogRequestStartAsync(
        TRequest request,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrEmpty(identifier))
        {
            // A logging error shouldn't stop execution
            return;
        }

        string requestName = typeof(TRequest).Name;
        _ = await _context.Log.AddAsync(new Log
        {
            RequestResponseName = requestName,
            RequestIdentifier = identifier,
            LogMessage = $"[STARTING] {requestName}",
            Timestamp = DateTime.UtcNow,
        }, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogRequestPropsAsync(
        TRequest request,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrEmpty(identifier))
        {
            // A logging error shouldn't stop execution
            return;
        }

        string requestName = typeof(TRequest).Name;
        _ = await _context.Log.AddAsync(new Log
        {
            RequestResponseName = requestName,
            RequestIdentifier = identifier,
            LogMessage = $"[PROPS] {JsonSerializer.Serialize(request)}",
            Timestamp = DateTime.UtcNow,
        }, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogRequestPropSerializationErrorAsync(
        TRequest request,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrEmpty(identifier))
        {
            // A logging error shouldn't stop execution
            return;
        }

        string requestName = typeof(TRequest).Name;
        _ = await _context.Log.AddAsync(new Log
        {
            RequestResponseName = requestName,
            RequestIdentifier = identifier,
            LogMessage = $"[Serialization ERROR] Could not serialize the request.",
            Timestamp = DateTime.UtcNow,
        }, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogRequestEndAsync(
        TResponse response,
        string identifier,
        Stopwatch stopwatch,
        CancellationToken cancellationToken = default)
    {
        if (response is null || string.IsNullOrEmpty(identifier) || stopwatch is null)
        {
            // A logging error shouldn't stop execution
            return;
        }

        string responseName = typeof(TResponse).Name;
        _ = await _context.Log.AddAsync(new Log
        {
            RequestResponseName = responseName,
            RequestIdentifier = identifier,
            LogMessage = $"[ENDED] {responseName}",
            Timestamp = DateTime.UtcNow,
            ElapsedTimeInMillis = stopwatch.ElapsedMilliseconds,
        }, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }
}