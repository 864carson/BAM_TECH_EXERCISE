using MediatR;
using MediatR.Pipeline;
using StargateAPI.Business.Data;
using StargateAPI.Business.Validators;
using StargateAPI.Controllers;
using StargateAPI.Repositories;
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
    /// <summary>Represents the person repository for interacting with the Stargate database.</summary>
    private readonly IStargateRepository _stargateRepository;

    /// <summary>Initializes a new instance of the UpdatePersonPreProcessor class.</summary>
    /// <param name="context">The StargateContext used for updating a person.</param>
    public UpdatePersonPreProcessor(IStargateRepository astronautRepository)
    {
        _stargateRepository = astronautRepository ?? throw new ArgumentNullException(nameof(astronautRepository));
    }

    /// <summary>
    /// Processes an update request for a person.
    /// </summary>
    /// <param name="request">The update request containing the new and current names of the person.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="BadHttpRequestException">
    /// Thrown when no person is found matching the current name in the request.
    /// /// </exception>
    public Task Process(UpdatePerson request, CancellationToken cancellationToken)
    {
        if (!UpdatePersonRequestValidator.IsValid(request))
        {
            throw new ArgumentException("A valid request object is required.", nameof(request));
        }

        // Make sure there is a person in the database with a matching name
        // Rule #1) A Person is uniquely identified by their name.
        Person? person = _stargateRepository.GetUntrackedAstronautByNameAsync(
            request.CurrentName,
            cancellationToken).Result;
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
/// This class implements the IRequestHandler interface to process the UpdatePerson request
/// and return an UpdatePersonResult.
/// </remarks>
/// <param name="context">The StargateContext for database operations.</param>
/// <returns>An UpdatePersonResult object with the updated person's ID.</returns>
public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
{
    /// <summary>Represents the person repository for interacting with the Stargate database.</summary>
    private readonly IStargateRepository _stargateRepository;

    /// <summary>Initializes a new instance of the UpdatePersonHandler class.</summary>
    /// <param name="context">The StargateContext used for updating a person.</param>
    public UpdatePersonHandler(IStargateRepository astronautRepository)
    {
        _stargateRepository = astronautRepository ?? throw new ArgumentNullException(nameof(astronautRepository));
    }

    /// <summary>
    /// Handles the updating of a person's name.
    /// </summary>
    /// <param name="request">The request containing the current and new names of the person.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An UpdatePersonResult object containing the updated person's ID.</returns>
    public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
    {
        if (!UpdatePersonRequestValidator.IsValid(request))
        {
            throw new ArgumentException("A valid request object is required.", nameof(request));
        }

        // We know there is a matching person record, because we made it through the preprocessor
        Person? existingPerson = (await _stargateRepository.GetAstronautByNameAsync(
            request.CurrentName,
            cancellationToken));

        // Rule #1) A Person is uniquely identified by their name.
        if (existingPerson is null)
        {
            return new UpdatePersonResult
            {
                Message = $"No person was found matching name '{request.CurrentName}'",
                Success = false,
                ResponseCode = (int)HttpStatusCode.NotFound
            };
        }

        // Update the person's name in the database
        _ = await _stargateRepository.UpdateAstronautNameAsync(
            existingPerson,
            request.NewName,
            cancellationToken);

        return new UpdatePersonResult
        {
            Id = existingPerson.Id
        };
    }
}
