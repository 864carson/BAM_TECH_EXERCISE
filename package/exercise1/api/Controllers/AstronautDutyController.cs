using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AstronautDutyController : ControllerBase
{
    /// <summary>
    /// Represents an instance of a mediator that facilitates communication between components.
    /// </summary>
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the AstronautDutyController class.
    /// </summary>
    /// <param name="mediator">The mediator used for communication between components.</param>
    public AstronautDutyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Retrieves astronaut duties by name.</summary>
    /// <param name="name">The name of the astronaut.</param>
    /// <returns>An asynchronous task that represents the operation and returns the astronaut duties.</returns>
    /// <response code="200">Returns the astronaut duties.</response>
    /// <response code="500">If an error occurs, returns an internal server error response.</response>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetAstronautDutiesByName(string name)
    {
        try
        {
            return this.GetResponse(await _mediator.Send(new GetAstronautDutiesByName
            {
                Name = name
            }));
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

    /// <summary>Creates a new astronaut duty based on the provided request.</summary>
    /// <param name="request">The request containing the details of the astronaut duty to be created.</param>
    /// <returns>An asynchronous task representing the operation, returning the result of the creation.</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
    {
        try
        {
            return this.GetResponse(await _mediator.Send(request));
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