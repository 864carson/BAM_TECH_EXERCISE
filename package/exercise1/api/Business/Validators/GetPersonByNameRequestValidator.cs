using StargateAPI.Business.Queries;

namespace StargateAPI.Business.Validators;

public static class GetPersonByNameRequestValidator
{
    public static bool IsValid(GetPersonByName person)
    {
        if (person is null || string.IsNullOrEmpty(person.Name))
        {
            return false;
        }

        return true;
    }
}