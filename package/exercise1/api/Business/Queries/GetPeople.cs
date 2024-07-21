using MediatR;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
using StargateAPI.Repositories;

namespace StargateAPI.Business.Queries;

/// <summary>
/// Represents a request to retrieve all people.
/// </summary>
public class GetPeople : IRequest<GetPeopleResult> { }

/// <summary>
/// Represents the result of a request to get all people.
/// </summary>
public class GetPeopleResult : BaseResponse
{
    /// <summary>Gets or sets all astronaut person information records.</summary>
    /// <value>All astronaut person information records. Default value is an empty list.</value>
    public List<PersonAstronaut> People { get; set; } = [];
}

public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
{
    /// <summary>Represents the person repository for interacting with the Stargate database.</summary>
    private readonly IStargateRepository _personRepository;

    /// <summary>Initializes a new instance of the GetPeopleHandler class.</summary>
    /// <param name="context">The StargateContext used for retrieving all people.</param>
    public GetPeopleHandler(IStargateRepository personRepo)
    {
        _personRepository = personRepo ?? throw new ArgumentNullException(nameof(personRepo));
    }

    /// <summary>
    /// Handles the request to retrieve all people.
    /// </summary>
    /// <param name="request">The empty request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation that returns the result of retrieving all people.
    /// </returns>
    public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
    {
        return new GetPeopleResult()
        {
            People = await _personRepository.GetAllAstronautDetailsAsync(cancellationToken)
        };
    }
}
