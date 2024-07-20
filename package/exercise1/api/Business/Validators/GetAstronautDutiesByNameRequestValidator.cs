using StargateAPI.Business.Queries;

namespace StargateAPI.Business.Validators;

public static class GetAstronautDutiesByNameRequestValidator
{
    public static bool IsValid(GetAstronautDutiesByName astronaut)
    {
        if (astronaut is null || string.IsNullOrEmpty(astronaut.Name))
        {
            return false;
        }

        return true;
    }
}