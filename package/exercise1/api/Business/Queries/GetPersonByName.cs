using System.Net;
using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

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
    /// <summary>Represents the context for interacting with the Stargate database.</summary>
    private readonly StargateContext _context;

    /// <summary>Initializes a new instance of the GetPersonByNameHandler class.</summary>
    /// <param name="context">The StargateContext used for retrieving a person.</param>
    public GetPersonByNameHandler(StargateContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to retrieve a person by name.
    /// </summary>
    /// <param name="request">The request containing the name of the person to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns the result of retrieving a person by name.</returns>
    public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
    {
        GetPersonByNameResult result = new();

        string query = @$"
            SELECT
                a.Id as PersonId
                , a.Name
                , b.CurrentRank
                , b.CurrentDutyTitle
                , b.CareerStartDate
                , b.CareerEndDate
            FROM [Person] a
            LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id
            WHERE '{request.Name.ToUpper()}' = UPPER(a.Name)";

        result.Person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(query);
        if (result.Person is null)
        {
            result.Message = $"No person was found matching name '{request.Name}'";
            result.Success = true;
            result.ResponseCode = (int)HttpStatusCode.NotFound;
        }

        return result;
    }
}
