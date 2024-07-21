namespace StargateAPI.Business.Dtos;

/// <summary>
/// The PersonAstronaut class defines properties for a person working as an astronaut.
/// </summary>
public class PersonAstronaut
{
    /// <summary>Gets or sets the id of the astronaut person object.</summary>
    public int PersonId { get; set; }

    /// <summary>Gets or sets the name of the astronaut person object.</summary>
    /// <value>The name of the astronaut person object. Default value is an empty string.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the current rank of the astronaut person object.</summary>
    /// <value>The current rank of the astronaut person object. Default value is an empty string.</value>
    public string CurrentRank { get; set; } = string.Empty;

    /// <summary>Gets or sets the current duty title of the astronaut person object.</summary>
    /// <value>The current duty title of the astronaut person object. Default value is an empty string.</value>
    public string CurrentDutyTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the career start date of the astronaut person object.</summary>
    public DateTime? CareerStartDate { get; set; }

    /// <summary>Gets or sets the career end date of the astronaut person object.</summary>
    public DateTime? CareerEndDate { get; set; }
}
