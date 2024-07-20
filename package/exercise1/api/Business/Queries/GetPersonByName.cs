using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
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
public class GetPersonByNameHandler : BaseQueryHandler<GetPersonByName, GetPersonByNameResult>
{
    /// <summary>Initializes a new instance of the GetPersonByNameHandler class.</summary>
    /// <param name="context">The StargateContext used for retrieving a person.</param>
    public GetPersonByNameHandler(StargateContext context) : base(context) { }

    /// <summary>
    /// Handles the request to retrieve a person by name.
    /// </summary>
    /// <param name="request">The request containing the name of the person to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns the result of retrieving a person by name.</returns>
    public async override Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
    {
        GetPersonByNameResult result = new()
        {
            Person = await (from p in _context.People
                            where p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)
                            from ad in _context.AstronautDetails.Where(x => x.PersonId == p.Id).DefaultIfEmpty()
                            select new PersonAstronaut
                            {
                                PersonId = p.Id,
                                Name = p.Name,
                                CurrentRank = ad.CurrentRank,
                                CurrentDutyTitle = ad.CurrentDutyTitle,
                                CareerStartDate = ad.CareerStartDate,
                                CareerEndDate = ad.CareerEndDate
                            }).FirstOrDefaultAsync(cancellationToken)
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
