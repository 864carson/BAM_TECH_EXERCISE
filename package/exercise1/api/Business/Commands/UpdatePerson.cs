using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands;

/// <summary>
/// The class UpdatePerson represents a request to update a person's name with properties for the
/// current name and the new name.
/// </summary>
public class UpdatePerson : IRequest<UpdatePersonResult>
{
    /// <summary>Represents the current name of the person to update.</summary>
    public required string CurrentName { get; set; } = string.Empty;

    /// <summary>Represents the new name of the person to update.</summary>
    public required string NewName { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of updating a person, extending the base response class.
/// </summary>
public class UpdatePersonResult : BaseResponse
{
    /// <summary>Represents the ID of the updated person.</summary>
    public int Id { get; set; }
}

/// <summary>
/// The UpdatePersonPreProcessor class names sure a person with the current name already exists in
/// the database before processing the request.
/// </summary>
public class UpdatePersonPreProcessor : IRequestPreProcessor<UpdatePerson>
{
    /// <summary>Represents the context for interacting with the Stargate database.</summary>
    private readonly StargateContext _context;

    /// <summary>Initializes a new instance of the UpdatePersonPreProcessor class.</summary>
    /// <param name="context">The StargateContext used for updating a person.</param>
    public UpdatePersonPreProcessor(StargateContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Processes an update request for a person.
    /// </summary>
    /// <param name="request">The update request containing the new and current names of the person.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="BadHttpRequestException">Thrown when no person is found matching the current name in the request.</exception>
    public Task Process(UpdatePerson request, CancellationToken cancellationToken)
    {
        // Make sure there is a person in the database with a matching name
        Person? person = _context.People.AsNoTracking().FirstOrDefault(x => x.Name.Equals(request.CurrentName, StringComparison.OrdinalIgnoreCase));
        if (person is null)
        {
            throw new BadHttpRequestException(
                $"No person was found matching name '{request.CurrentName}'",
                (int)HttpStatusCode.NotFound
            );
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the request to update a person's information.
/// </summary>
/// <typeparam name="UpdatePerson">The request type to update a person.</typeparam>
/// <typeparam name="UpdatePersonResult">The result type after updating a person.</typeparam>
/// <remarks>
/// This class implements the IRequestHandler interface to process the UpdatePerson request and return an UpdatePersonResult.
/// </remarks>
/// <param name="context">The StargateContext for database operations.</param>
/// <returns>An UpdatePersonResult object with the updated person's ID.</returns>
public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
{
    /// <summary>Represents the context for interacting with the Stargate database.</summary>
    private readonly StargateContext _context;

    /// <summary>Initializes a new instance of the UpdatePersonHandler class.</summary>
    /// <param name="context">The StargateContext used for updating a person.</param>
    public UpdatePersonHandler(StargateContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the updating of a person's name.
    /// </summary>
    /// <param name="request">The request containing the current and new names of the person.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An UpdatePersonResult object containing the updated person's ID.</returns>
    public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
    {
        // We know there is a matching person record, because we made it through the preprocessor
        Person existingPerson = await _context.People.FirstAsync(x => x.Name.Equals(request.CurrentName, StringComparison.OrdinalIgnoreCase));

        // Update the person's name in the database
        existingPerson.Name = request.NewName;
        _ = await _context.SaveChangesAsync(cancellationToken);

        return new UpdatePersonResult
        {
            Id = existingPerson.Id
        };
    }
}
