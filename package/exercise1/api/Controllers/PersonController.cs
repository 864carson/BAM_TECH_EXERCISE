using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers;

[ApiController]
[Route("[controller]")]
/// <summary>
/// Controller for managing person-related operations.
/// </summary>
public class PersonController : ControllerBase
{
    /// <summary>Interface for mediating between components.</summary>
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the PersonController class.
    /// </summary>
    /// <param name="mediator">The mediator used for communication between components.</param>
    public PersonController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get a list of people.</summary>
    /// <returns>An IActionResult representing the list of people.</returns>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("")]
    public async Task<IActionResult> GetPeople()
    {
        try
        {
            return this.GetResponse(await _mediator.Send(new GetPeople()));
        }
        catch (Exception ex)
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }

    /// <summary>Get a person by name.</summary>
    /// <param name="name">The name of the person to retrieve.</param>
    /// <returns>An IActionResult representing the result of the operation.</returns>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetPersonByName(string name)
    {
        try
        {
            return this.GetResponse(await _mediator.Send(new GetPersonByName
            {
                Name = name
            }));
        }
        catch (BadHttpRequestException ex)
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }

    /// <summary>Creates a new person with the given name.</summary>
    /// <param name="name">The name of the person to create.</param>
    /// <returns>An asynchronous task representing the operation result.</returns>
    /// <response code="200">Returns the newly created person.</response>
    /// <response code="500">If an error occurs during the creation process.</response>
    [HttpPost("")]
    public async Task<IActionResult> CreatePerson([FromBody] string name)
    {
        try
        {
            return this.GetResponse(await _mediator.Send(new CreatePerson
            {
                Name = name
            }));
        }
        catch (BadHttpRequestException ex)
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }

    /// <summary>
    /// Updates a person's name based on the provided current name.
    /// </summary>
    /// <param name="name">The name of the person to update.</param>
    /// <param name="newName">The updated name of the person.</param>
    /// <returns>An asynchronous task representing the action result.</returns>
    [HttpPut("{name}")]
    public async Task<IActionResult> UpdatePerson(string name, [FromBody] string newName)
    {
        try
        {
            return this.GetResponse(await _mediator.Send(new UpdatePerson
            {
                CurrentName = name,
                NewName = newName
            }));
        }
        catch (BadHttpRequestException ex)
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }
}