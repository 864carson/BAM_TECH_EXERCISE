using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

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
public class CreatePersonPreProcessor : BasePreProcessor<CreatePerson, CreatePersonResult>
{
    /// <summary>Initializes a new instance of the CreatePersonPreProcessor class.</summary>
    /// <param name="context">The StargateContext used for creating a person.</param>
    public CreatePersonPreProcessor(StargateContext context) : base(context) { }

    /// <summary>
    /// Processes an create request for a person.
    /// </summary>
    /// <param name="request">The create request containing the name of the person.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="BadHttpRequestException">
    /// Thrown when a person is found matching the name in the request.
    /// </exception>
    public override Task Process(CreatePerson request, CancellationToken cancellationToken)
    {
        // Make sure there is not a person in the database with a matching name
        Person? person = _context.People
            .AsNoTracking()
            .FirstOrDefault(x => x.Name.ToUpper() == request.Name.ToUpper());
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
public class CreatePersonHandler : BaseCommandHandler<CreatePerson, CreatePersonResult>
{
     /// <summary>Initializes a new instance of the CreatePersonHandler class.</summary>
    /// <param name="context">The StargateContext used for creating a person.</param>
    public CreatePersonHandler(StargateContext context) : base(context) { }

    /// <summary>
    /// Handles the creating of a person.
    /// </summary>
    /// <param name="request">The request containing the name of the person.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An CreatePersonResult object containing the created person's ID.</returns>
    public async override Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
    {
        // Create the new person
        Person newPerson = new()
        {
            Name = request.Name
        };

        // Add the new person to the database
        _ = await _context.People.AddAsync(newPerson, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);

        return new CreatePersonResult()
        {
            Id = newPerson.Id
        };
    }
}
