using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Validators;
using StargateAPI.Controllers;
using StargateAPI.Repositories;
using System.Net;

namespace StargateAPI.Business.Queries;

/// <summary>
/// Represents a request to retrieve astronaut duties by astronaut name.
/// </summary>
public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
{
    /// <summary>Represents the name of the person.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of a request to get astronaut duties by astronaut name.
/// </summary>
public class GetAstronautDutiesByNameResult : BaseResponse
{
    /// <summary>Gets or sets the astronaut person information.</summary>
    public PersonAstronaut? Person { get; set; }

    /// <summary>Gets or sets the astronaut duty information list.</summary>
    public List<AstronautDuty> AstronautDuties { get; set; } = [];
}

/// <summary>
/// Handles the request to retrieve astronaut duties by astronaut name from the database.
/// </summary>
/// <typeparam name="GetAstronautDutiesByName">
/// The request type to get astronaut duties by astronaut name.
/// </typeparam>
/// <typeparam name="GetAstronautDutiesByNameResult">
/// The result type of getting astronaut duties by astronaut name.
/// </typeparam>
public class GetAstronautDutiesByNameHandler
    : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
{
    /// <summary>Represents the person repository for interacting with the Stargate database.</summary>
    private readonly IStargateRepository _personRepository;

    /// <summary>Initializes a new instance of the GetAstronautDutiesByNameHandler class.</summary>
    /// <param name="context">The StargateContext used for retrieving astronaut duties by astronaut name.</param>
    public GetAstronautDutiesByNameHandler(IStargateRepository personRepo)
    {
        _personRepository = personRepo ?? throw new ArgumentNullException(nameof(personRepo));
    }

    /// <summary>
    /// Handles the request to retrieve astronaut duties by astronaut name.
    /// </summary>
    /// <param name="request">The request containing the name of the person to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation that returns the result of retrieving astronaut duties by astronaut name.
    /// </returns>
    public async Task<GetAstronautDutiesByNameResult> Handle(
        GetAstronautDutiesByName request,
        CancellationToken cancellationToken)
    {
        if (!GetAstronautDutiesByNameRequestValidator.IsValid(request))
        {
            throw new ArgumentException("A valid request object is required.", nameof(request));
        }

        GetAstronautDutiesByNameResult result = new();
        PersonAstronaut? person =
            await _personRepository.GetAstronautDetailsByNameAsync(request.Name, cancellationToken);
        if (person is null)
        {
            result.Message = $"No person was found matching name '{request.Name}'";
            result.Success = true;
            result.ResponseCode = (int)HttpStatusCode.NotFound;
            return result;
        }
        result.Person = person!;

        result.AstronautDuties =
            await _personRepository.GetAstronautDutiesByAstronautIdAsync(result.Person.PersonId);

        return result;
    }
}
