using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

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
    /// <summary>Represents the context for interacting with the Stargate database.</summary>
    public readonly StargateContext _context;

    /// <summary>Initializes a new instance of the GetPeopleHandler class.</summary>
    /// <param name="context">The StargateContext used for retrieving all people.</param>
    public GetPeopleHandler(StargateContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to retrieve all people.
    /// </summary>
    /// <param name="request">The empty request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns the result of retrieving all people.</returns>
    public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
    {
        GetPeopleResult result = new();

        string query = @$"
            SELECT
                a.Id as PersonId
                , a.Name
                , b.CurrentRank
                , b.CurrentDutyTitle
                , b.CareerStartDate
                , b.CareerEndDate
            FROM [Person] a
            LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id";

        result.People = (await _context.Connection.QueryAsync<PersonAstronaut>(query)).ToList();
        return result;
    }
}
