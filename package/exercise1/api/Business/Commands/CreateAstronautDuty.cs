using MediatR;
using MediatR.Pipeline;
using StargateAPI.Business.Data;
using StargateAPI.Business.Validators;
using StargateAPI.Controllers;
using StargateAPI.Repositories;
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
public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
{
    /// <summary>Represents the logic-specific part of the application configuration.</summary>
    private readonly AppConfig _config;

    /// <summary>Represents the person repository for interacting with the Stargate database.</summary>
    private readonly IStargateRepository _stargateRepository;

    /// <summary>
    /// Initializes a new instance of the CreateAstronautDutyPreProcessor class with the specified StargateContext.
    /// </summary>
    /// <param name="context">The StargateContext used for processing.</param>
    public CreateAstronautDutyPreProcessor(IStargateRepository astronautRepository, AppConfig config)
    {
        _stargateRepository = astronautRepository ?? throw new ArgumentNullException(nameof(astronautRepository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Processes the creation of an astronaut duty based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the details of the astronaut duty to be created.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="BadHttpRequestException">Thrown when a person with the same name already has an astronaut duty or when no person is found with the given name.</exception>
    public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
    {
        if (!CreateAstronautDutyRequestValidator.IsValid(_config, request))
        {
            throw new ArgumentException("A valid request object is required.", nameof(request));
        }

        Person? person = _stargateRepository
            .GetUntrackedAstronautByNameAsync(request.Name, cancellationToken)
            .Result;
        if (person is null)
        {
            throw new BadHttpRequestException(
                $"No person was found matching name '{request.Name}'",
                (int)HttpStatusCode.NotFound);
        }

        // Verify no duty for this astronaut matches the specified title and start date.
        // As the logic is written below, this SHOULD ALWAYS BE NULL due to the start date being a timestamp.
        AstronautDuty? verifyNoPreviousDuty = _stargateRepository.GetAstronautDutyByIdTitleStartDateAsync(
            person.Id,
            request.DutyTitle,
            request.DutyStartDate,
            cancellationToken)
            .Result;
        if (verifyNoPreviousDuty is not null)
        {
            throw new BadHttpRequestException(
                $"'{request.Name}' has previous Astronaut Duty",
                (int)HttpStatusCode.BadRequest);
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
public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
{
    /// <summary>Represents the person repository for interacting with the Stargate database.</summary>
    private readonly IStargateRepository _stargateRepository;

    /// <summary>Represents the logic-specific part of the application configuration.</summary>
    private readonly AppConfig _config;

    /// <summary>Initializes a new instance of the CreateAstronautDutyHandler class.</summary>
    /// <param name="context">The StargateContext to be used for database operations.</param>
    public CreateAstronautDutyHandler(IStargateRepository astronautRepository, AppConfig config)
    {
        _stargateRepository = astronautRepository ?? throw new ArgumentNullException(nameof(astronautRepository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Handles the creation of an astronaut duty based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the astronaut's name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, returning the result of creating the astronaut duty.</returns>
    public async Task<CreateAstronautDutyResult> Handle(
        CreateAstronautDuty request,
        CancellationToken cancellationToken)
    {
        if (!CreateAstronautDutyRequestValidator.IsValid(_config, request))
        {
            throw new ArgumentException("A valid request object is required.", nameof(request));
        }

        // We know there is a matching person record, because we made it through the preprocessor
        Person? person = await _stargateRepository
            .GetUntrackedAstronautByNameAsync(request.Name, cancellationToken);
        if (person is null)
        {
            return new CreateAstronautDutyResult()
            {
                Message = $"No person was found matching name '{request.Name}'",
                Success = false,
                ResponseCode = (int)HttpStatusCode.NotFound
            };
        }

        // Get the astronaut's detail
        AstronautDetail? astronautDetail = await _stargateRepository
            .GetAstronautDetailByIdAsync(person.Id, cancellationToken);
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
            _ = await _stargateRepository
                .AddAstronautDetailAsync(astronautDetail, cancellationToken);
        }
        else
        {
            if (request.DutyTitle == _config.RetiredDutyTitle)
            {
                astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
            }

            // Update the astronaut's details
            _ = await _stargateRepository
                .UpdateAstronautDetailAsync(astronautDetail, request, cancellationToken);
        }

        // Get the astronaut's duty records
        AstronautDuty? astronautDuty = await _stargateRepository
            .GetMostRecentAstronautDutyByAstronautIdAsync(person.Id, cancellationToken);
        if (astronautDuty is not null)
        {
            astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;

            // Update the astronaut's duty record end date
            _ = await _stargateRepository
                .UpdateAstronautDutyAsync(astronautDuty, cancellationToken);
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
        _ = await _stargateRepository
            .AddAstronautDutyAsync(newAstronautDuty, cancellationToken);

        return new CreateAstronautDutyResult()
        {
            Id = newAstronautDuty.Id
        };
    }
}
