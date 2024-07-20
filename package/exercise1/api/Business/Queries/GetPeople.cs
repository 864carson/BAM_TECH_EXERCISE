using MediatR;
using Microsoft.EntityFrameworkCore;
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

public class GetPeopleHandler : BaseQueryHandler<GetPeople, GetPeopleResult>
{
    /// <summary>Initializes a new instance of the GetPeopleHandler class.</summary>
    /// <param name="context">The StargateContext used for retrieving all people.</param>
    public GetPeopleHandler(StargateContext context) : base(context) { }

    /// <summary>
    /// Handles the request to retrieve all people.
    /// </summary>
    /// <param name="request">The empty request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns the result of retrieving all people.</returns>
    public async override Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
    {
        GetPeopleResult result = new()
        {
            People = await (from p in _context.People
                            from ad in _context.AstronautDetails.Where(x => x.PersonId == p.Id).DefaultIfEmpty()
                            select new PersonAstronaut
                            {
                                PersonId = p.Id,
                                Name = p.Name,
                                CurrentRank = ad.CurrentRank,
                                CurrentDutyTitle = ad.CurrentDutyTitle,
                                CareerStartDate = ad.CareerStartDate,
                                CareerEndDate = ad.CareerEndDate
                            }).ToListAsync(cancellationToken: cancellationToken)
        };

        return result;
    }
}
