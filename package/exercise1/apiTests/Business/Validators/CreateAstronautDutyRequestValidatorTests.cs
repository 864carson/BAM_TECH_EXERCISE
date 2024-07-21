using StargateAPI;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Validators;

namespace apiTests.Business.Validators;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8601 // Possible null reference assignment.
public class CreateAstronautDutyRequestValidatorTests
{
    private AppConfig _config;

    [SetUp]
    public void Setup()
    {
        _config = new AppConfig();
    }

    [Test]
    public void IsValid_NullCreateAstronautDuty_Test()
    {
        bool result = CreateAstronautDutyRequestValidator.IsValid(_config, null);
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void IsValid_NullOrInvalidName_Test(string? name)
    {
        bool result = CreateAstronautDutyRequestValidator.IsValid(_config, new CreateAstronautDuty
        {
            Name = name,
            Rank = "Corporal",
            DutyTitle = "Mess Master",
            DutyStartDate = DateTime.Now
        });
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void IsValid_NullOrInvalidRank_Test(string? rank)
    {
        bool result = CreateAstronautDutyRequestValidator.IsValid(_config, new CreateAstronautDuty
        {
            Name = "Maverick",
            Rank = rank,
            DutyTitle = "Mess Master",
            DutyStartDate = DateTime.Now
        });
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void IsValid_NullOrInvalidDutyTitle_Test(string? dutyTitle)
    {
        bool result = CreateAstronautDutyRequestValidator.IsValid(_config, new CreateAstronautDuty
        {
            Name = "Maverick",
            Rank = "Corporal",
            DutyTitle = dutyTitle,
            DutyStartDate = DateTime.Now
        });
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_NullDutyStartDate_Test()
    {
        bool result = CreateAstronautDutyRequestValidator.IsValid(_config, new CreateAstronautDuty
        {
            Name = "Maverick",
            Rank = "Corporal",
            DutyTitle = "Mess Master",
            DutyStartDate = default
        });
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase("12/31/1899")]
    [TestCase("1/1/1800")]
    public void IsValid_InvalidDutyStartDate_Test(string startDateString)
    {
        bool result = CreateAstronautDutyRequestValidator.IsValid(_config, new CreateAstronautDuty
        {
            Name = "Maverick",
            Rank = "Corporal",
            DutyTitle = "Mess Master",
            DutyStartDate = DateTime.Parse(startDateString)
        });
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase("1/1/1900")]
    [TestCase("12/31/1999")]
    [TestCase("1/1/2000")]
    [TestCase("12/31/2999")]
    [TestCase("1/1/3000")]
    public void IsValid_ValidCreateAstronautDuty_Test(string startDateString)
    {
        bool result = CreateAstronautDutyRequestValidator.IsValid(_config, new CreateAstronautDuty
        {
            Name = "Maverick",
            Rank = "Corporal",
            DutyTitle = "Mess Master",
            DutyStartDate = DateTime.Parse(startDateString)
        });
        Assert.That(result, Is.True);
    }
}
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
