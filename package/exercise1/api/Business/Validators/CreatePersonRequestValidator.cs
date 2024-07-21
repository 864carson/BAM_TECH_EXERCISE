using StargateAPI.Business.Commands;

namespace StargateAPI.Business.Validators;

public static class CreatePersonRequestValidator
{
    public static bool IsValid(CreatePerson createPerson)
    {
        if (createPerson is null || string.IsNullOrEmpty(createPerson.Name))
        {
            return false;
        }

        return true;
    }
}