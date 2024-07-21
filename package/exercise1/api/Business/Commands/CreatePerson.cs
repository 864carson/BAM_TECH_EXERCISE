using MediatR;
using MediatR.Pipeline;
using StargateAPI.Business.Data;
using StargateAPI.Business.Validators;
using StargateAPI.Controllers;
using StargateAPI.Repositories;
using System.Net;

namespace StargateAPI.Business.Commands;

/// <summary>
/// The class CreatePerson represents a request to create a person with properties for the
/// person's name.
/// </summary>
public class CreatePerson : IRequest<CreatePersonResult>
{
    /// <summary>Represents the name of the person to create.</summary>
    public required string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of creating a person, extending the base response class.
/// </summary>
public class CreatePersonResult : BaseResponse
{
    /// <summary>Represents the ID of the created person.</summary>
    public int Id { get; set; }
}

/// <summary>
/// The CreatePersonPreProcessor class names sure a person with the current name does not already exists in
/// the database before processing the request.
/// </summary>
public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
{
    /// <summary>Represents the person repository for interacting with the Stargate database.</summary>
    private readonly IStargateRepository _stargateRepository;

    /// <summary>Initializes a new instance of the CreatePersonPreProcessor class.</summary>
    /// <param name="context">The StargateContext used for creating a person.</param>
    public CreatePersonPreProcessor(IStargateRepository astronautRepository)
    {
        _stargateRepository = astronautRepository ?? throw new ArgumentNullException(nameof(astronautRepository));
    }

    /// <summary>
    /// Processes an create request for a person.
    /// </summary>
    /// <param name="request">The create request containing the name of the person.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="BadHttpRequestException">
    /// Thrown when a person is found matching the name in the request.
    /// </exception>
    public Task Process(CreatePerson request, CancellationToken cancellationToken)
    {
        if (!CreatePersonRequestValidator.IsValid(request))
        {
            throw new ArgumentException("A valid request object is required.", nameof(request));
        }

        // Make sure there is not a person in the database with a matching name
        // Rule #1) A Person is uniquely identified by their name.
        Person? person = _stargateRepository.GetUntrackedAstronautByNameAsync(
            request.Name,
            cancellationToken).Result;
        if (person is not null)
        {
            throw new BadHttpRequestException(
                $"A person already exists with name matching '{request.Name}'",
                (int)HttpStatusCode.BadRequest
            );
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the request to create a person's information.
/// </summary>
/// <typeparam name="CreatePerson">The request type to create a person.</typeparam>
/// <typeparam name="CreatePersonResult">The result type after creating a person.</typeparam>
/// <remarks>
/// This class implements the IRequestHandler interface to process the CreatePerson request
/// and return an CreatePersonResult.
/// </remarks>
/// <param name="context">The StargateContext for database operations.</param>
/// <returns>An CreatePersonResult object with the created person's ID.</returns>
public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
{
    /// <summary>Represents the person repository for interacting with the Stargate database.</summary>
    private readonly IStargateRepository _stargateRepository;

    /// <summary>Initializes a new instance of the CreatePersonHandler class.</summary>
    /// <param name="context">The StargateContext used for creating a person.</param>
    public CreatePersonHandler(IStargateRepository astronautRepository)
    {
        _stargateRepository = astronautRepository ?? throw new ArgumentNullException(nameof(astronautRepository));
    }

    /// <summary>
    /// Handles the creating of a person.
    /// </summary>
    /// <param name="request">The request containing the name of the person.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An CreatePersonResult object containing the created person's ID.</returns>
    public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
    {
        if (!CreatePersonRequestValidator.IsValid(request))
        {
            throw new ArgumentException("A valid request object is required.", nameof(request));
        }

        // Create the new person
        Person newPerson = new()
        {
            Name = request.Name
        };

        // Add the new person to the database
        _ = await _stargateRepository.AddAstronautAsync(newPerson, cancellationToken);

        return new CreatePersonResult()
        {
            Id = newPerson.Id
        };
    }
}
