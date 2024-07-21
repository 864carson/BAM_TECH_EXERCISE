using StargateAPI.Business.Commands;

namespace StargateAPI.Business.Validators;

public static class CreateAstronautDutyRequestValidator
{
    public static bool IsValid(AppConfig config, CreateAstronautDuty astronautDuty)
    {
        if (astronautDuty is null
            || string.IsNullOrEmpty(astronautDuty.Name)
            || string.IsNullOrEmpty(astronautDuty.Rank)
            || string.IsNullOrEmpty(astronautDuty.DutyTitle)
            || astronautDuty.DutyStartDate < config.MinDutyStartDate)
        {
            return false;
        }

        return true;
    }
}