using MediatR;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Validators;
using StargateAPI.Controllers;
using StargateAPI.Repositories;
using System.Net;

namespace StargateAPI.Business.Queries;

/// <summary>
/// Represents a request to retrieve a person by name.
/// </summary>
public class GetPersonByName : IRequest<GetPersonByNameResult>
{
    /// <summary>Gets or sets the name of the person to retrieve.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of a request to get a person by name.
/// </summary>
public class GetPersonByNameResult : BaseResponse
{
    /// <summary>Gets or sets the astronaut person information.</summary>
    public PersonAstronaut? Person { get; set; }
}

/// <summary>
/// Handles the request to retrieve a person by name from the database.
/// </summary>
/// <typeparam name="GetPersonByName">The request type to get a person by name.</typeparam>
/// <typeparam name="GetPersonByNameResult">The result type of getting a person by name.</typeparam>
public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
{
    /// <summary>Represents the person repository for interacting with the Stargate database.</summary>
    private readonly IStargateRepository _personRepository;

    /// <summary>Initializes a new instance of the GetPersonByNameHandler class.</summary>
    /// <param name="context">The StargateContext used for retrieving a person.</param>
    public GetPersonByNameHandler(IStargateRepository personRepo)
    {
        _personRepository = personRepo ?? throw new ArgumentNullException(nameof(personRepo));
    }

    /// <summary>
    /// Handles the request to retrieve a person by name.
    /// </summary>
    /// <param name="request">The request containing the name of the person to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation that returns the result of retrieving a person by name.
    /// </returns>
    public async Task<GetPersonByNameResult> Handle(
        GetPersonByName request,
        CancellationToken cancellationToken)
    {
        if (!GetPersonByNameRequestValidator.IsValid(request))
        {
            throw new ArgumentException("A valid request object is required.", nameof(request));
        }

        GetPersonByNameResult result = new()
        {
            Person = await _personRepository.GetAstronautDetailsByNameAsync(
                request.Name,
                cancellationToken)
        };
        if (result.Person is null)
        {
            result.Message = $"No person was found matching name '{request.Name}'";
            result.Success = true;
            result.ResponseCode = (int)HttpStatusCode.NotFound;
        }

        return result;
    }
}
