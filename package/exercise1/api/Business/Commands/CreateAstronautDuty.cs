using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands;

/// <summary>
/// Represents a request to create an astronaut duty.
/// </summary>
public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
{
    /// <summary>Gets or sets the name.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the rank of an item.</summary>
    public required string Rank { get; set; }

    /// <summary>Gets or sets the duty title.</summary>
    public required string DutyTitle { get; set; }

    /// <summary>Gets or sets the start date of duty.</summary>
    public DateTime DutyStartDate { get; set; }
}

/// <summary>Represents the result of creating an astronaut duty.</summary>
public class CreateAstronautDutyResult : BaseResponse
{
    /// <summary>Gets or sets the ID of the astronaut duty.</summary>
    public int? Id { get; set; }
}

/// <summary>
/// Preprocessor for creating an astronaut duty.
/// </summary>
/// <typeparam name="CreateAstronautDuty">The type of the request to create an astronaut duty.</typeparam>
/// <typeparam name="CreateAstronautDutyResult">The type of the result after creating an astronaut duty.</typeparam>
/// <remarks>
/// This preprocessor checks if there is any previous duty for the astronaut before creating a new one.
/// </remarks>
/// <param name="context">The Stargate context.</param>
/// <returns>A task representing the asynchronous operation.</returns>
/// <exception cref="BadHttpRequestException">Thrown when the astronaut has a previous duty.</exception>
public class CreateAstronautDutyPreProcessor
    : BasePreProcessor<CreateAstronautDuty, CreateAstronautDutyResult>
{
    /// <summary>
    /// Initializes a new instance of the CreateAstronautDutyPreProcessor class with the specified StargateContext.
    /// </summary>
    /// <param name="context">The StargateContext used for processing.</param>
    public CreateAstronautDutyPreProcessor(StargateContext context) : base(context) { }

    /// <summary>
    /// Processes the creation of an astronaut duty based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the details of the astronaut duty to be created.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="BadHttpRequestException">Thrown when a person with the same name already has an astronaut duty or when no person is found with the given name.</exception>
    public override Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
    {
        Person? person = Utils.GetUntrackedPersonByName(_context, request.Name);
        if (person is null)
        {
            throw new BadHttpRequestException(
                $"No person was found matching name '{request.Name}'",
                (int)HttpStatusCode.NotFound
            );
        }

        // Verify the astronaut has no previous duty matching the specified title and start date.
        // As the logic is written below, this should always be null due to the start date being a timestamp.
        AstronautDuty? verifyNoPreviousDuty = _context.AstronautDuties
            .FirstOrDefault(x => x.DutyTitle == request.DutyTitle && x.DutyStartDate == request.DutyStartDate);
        if (verifyNoPreviousDuty is not null)
        {
            throw new BadHttpRequestException(
                $"'{request.Name}' has previous Astronaut Duty",
                (int)HttpStatusCode.BadRequest
            );
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the command to create a new astronaut duty.
/// </summary>
/// <param name="command">The command to create a new astronaut duty.</param>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>The result of creating a new astronaut duty.</returns>
public class CreateAstronautDutyHandler : BaseCommandHandler<CreateAstronautDuty, CreateAstronautDutyResult>
{
    private readonly AppConfig _config;

    /// <summary>Initializes a new instance of the CreateAstronautDutyHandler class.</summary>
    /// <param name="context">The StargateContext to be used for database operations.</param>
    public CreateAstronautDutyHandler(AppConfig config, StargateContext context) : base(context)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Handles the creation of an astronaut duty based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the astronaut's name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, returning the result of creating the astronaut duty.</returns>
    public async override Task<CreateAstronautDutyResult> Handle(
        CreateAstronautDuty request,
        CancellationToken cancellationToken)
    {
        // We know there is a matching person record, because we made it through the preprocessor
        Person person = Utils.GetUntrackedPersonByName(_context, request.Name)!;

        // Get the astronaut's detail
        AstronautDetail? astronautDetail = await _context.AstronautDetails.FirstOrDefaultAsync(x => x.PersonId == person.Id);
        if (astronautDetail is null)
        {
            astronautDetail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentDutyTitle = request.DutyTitle,
                CurrentRank = request.Rank,
                CareerStartDate = request.DutyStartDate.Date
            };

            if (request.DutyTitle == _config.RetiredDutyTitle)
            {
                astronautDetail.CareerEndDate = request.DutyStartDate.Date;
            }

            // Save the astronaut's details
            _ = await _context.AstronautDetails.AddAsync(astronautDetail, cancellationToken);
        }
        else
        {
            astronautDetail.CurrentDutyTitle = request.DutyTitle;
            astronautDetail.CurrentRank = request.Rank;
            if (request.DutyTitle == _config.RetiredDutyTitle)
            {
                astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
            }

            // Update the astronaut's details
            _ = _context.AstronautDetails.Update(astronautDetail);
        }

        // Get the astronaut's duty records
        AstronautDuty? astronautDuty = await _context.AstronautDuties
            .OrderByDescending(x => x.DutyStartDate)
            .FirstOrDefaultAsync(x => x.PersonId == person.Id);
        if (astronautDuty is not null)
        {
            astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;

            // Update the astronaut's duty record end date
            _context.AstronautDuties.Update(astronautDuty);
        }

        // Create and add the new duty record to the database
        AstronautDuty newAstronautDuty = new()
        {
            PersonId = person.Id,
            Rank = request.Rank,
            DutyTitle = request.DutyTitle,
            DutyStartDate = request.DutyStartDate.Date,
            DutyEndDate = null
        };

        _ = await _context.AstronautDuties.AddAsync(newAstronautDuty, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);

        return new CreateAstronautDutyResult()
        {
            Id = newAstronautDuty.Id
        };
    }
}

/// <summary>
/// Retrieves a person from the database without tracking by name.
/// </summary>
/// <param name="context">The database context.</param>
/// <param name="name">The name of the person to retrieve.</param>
/// <returns>The person with the specified name, or null if not found or if the name is empty.</returns>
internal static class Utils
{
    /// <summary>
    /// Retrieves a person by name without tracking in the database.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="name">The name of the person to retrieve.</param>
    /// <returns>
    /// The person with the specified name, or null if the name is empty or the person is not found.
    /// </returns>
    internal static Person? GetUntrackedPersonByName(StargateContext context, string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        return context.People
            .AsNoTracking()
            .FirstOrDefault(x => x.Name.ToUpper() == name.ToUpper());
    }
}
