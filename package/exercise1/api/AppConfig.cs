namespace StargateAPI;

public class AppConfig
{
    /// <summary>Gets or sets the duty title to be used when an astronaut retires from service.</summary>
    /// <value>The duty title to be used when an astronaut retires from service. Default value is RETIRED.</value>
    public string RetiredDutyTitle { get; set; } = "RETIRED";

    /// <summary>Gets or sets the earliest acceptable duty start date.</summary>
    /// <value>The earliest acceptable duty start date. Default value is 1/1/1900.</value>
    public DateTime MinDutyStartDate { get; set; } = new DateTime(1900, 1, 1);
}