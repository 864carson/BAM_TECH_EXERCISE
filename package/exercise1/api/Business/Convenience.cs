using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Business;

/// <summary>
/// Retrieves a person from the database without tracking by name.
/// </summary>
/// <param name="context">The database context.</param>
/// <param name="name">The name of the person to retrieve.</param>
/// <returns>The person with the specified name, or null if not found or if the name is empty.</returns>
public static class Convenience
{
    /// <summary>
    /// Retrieves a person by name without tracking in the database.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="name">The name of the person to retrieve.</param>
    /// <returns>
    /// The person with the specified name, or null if the name is empty or the person is not found.
    /// </returns>
    public static Person? GetUntrackedPersonByName(StargateContext context, string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        return context.People
            .AsNoTracking()
            .FirstOrDefault(x => x.Name.ToUpper().Equals(name.ToUpper()));
    }
}
