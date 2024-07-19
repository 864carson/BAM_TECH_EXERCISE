using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace StargateAPI.Controllers;

/// <summary>
/// Extension method for ControllerBase to generate an IActionResult response based on a BaseResponse object.
/// </summary>
/// <param name="controllerBase">The ControllerBase instance.</param>
/// <param name="response">The BaseResponse object containing response data.</param>
/// <param name="statusCode">Optional HTTP status code for the response.</param>
/// <returns>An IActionResult response based on the provided BaseResponse object.</returns>
public static class ControllerBaseExtensions
{
    /// <summary>
    /// Generates an HTTP response based on the provided BaseResponse object.
    /// </summary>
    /// <param name="controllerBase">The controller instance.</param>
    /// <param name="response">The BaseResponse object containing response data.</param>
    /// <param name="statusCode">Optional HTTP status code to be returned.</param>
    /// <returns>An IActionResult representing the HTTP response.</returns>
    public static IActionResult GetResponse(
        this ControllerBase controllerBase,
        BaseResponse response,
        HttpStatusCode? statusCode = null)
    {
        ObjectResult httpResponse = new (response)
        {
            StatusCode = statusCode == null ? response.ResponseCode : (int)statusCode
        };
        return httpResponse;
    }
}