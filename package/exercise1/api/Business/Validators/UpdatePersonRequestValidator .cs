using StargateAPI.Business.Commands;

namespace StargateAPI.Business.Validators;

public static class UpdatePersonRequestValidator
{
    public static bool IsValid(UpdatePerson createPerson)
    {
        if (createPerson is null
            || string.IsNullOrEmpty(createPerson.CurrentName)
            || string.IsNullOrEmpty(createPerson.NewName))
        {
            return false;
        }

        return true;
    }
}