using StargateAPI.Business.Queries;
using StargateAPI.Business.Validators;

namespace apiTests.Business.Validators;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8601 // Possible null reference assignment.
public class GetAstronautDutiesByNameRequestValidatorTests
{
    [Test]
    public void IsValid_NullGetPersonByName_Test()
    {
        bool result = GetAstronautDutiesByNameRequestValidator.IsValid(null);
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void IsValid_NullOrInvalidName_Test(string? name)
    {
        bool result = GetAstronautDutiesByNameRequestValidator.IsValid(new GetAstronautDutiesByName
        {
            Name = name
        });
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase(" ")]
    [TestCase("Name1")]
    public void IsValid_ValidName_Test(string name)
    {
        bool result = GetAstronautDutiesByNameRequestValidator.IsValid(new GetAstronautDutiesByName
        {
            Name = name
        });
        Assert.That(result, Is.True);
    }
}
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
