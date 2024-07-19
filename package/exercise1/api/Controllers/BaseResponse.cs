using System.Net;

namespace StargateAPI.Controllers;

/// <summary>
/// Represents a base response object containing success status, message, and response code.
/// </summary>
public class BaseResponse
{
    /// <summary>Gets or sets a value indicating whether the operation was successful.</summary>
    public bool Success { get; set; } = true;

    /// <summary>Gets or sets the message.</summary>
    /// <value>The message. Default value is "Successful"</value>
    public string Message { get; set; } = "Successful";

    /// <summary>Gets or sets the HTTP response code.</summary>
    /// <value>The HTTP response code. Default value is 200 (OK).</value>
    public int ResponseCode { get; set; } = (int)HttpStatusCode.OK;
}