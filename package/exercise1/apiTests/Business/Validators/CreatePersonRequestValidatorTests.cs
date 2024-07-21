using StargateAPI.Business.Commands;
using StargateAPI.Business.Validators;

namespace apiTests.Business.Validators;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8601 // Possible null reference assignment.
public class CreatePersonRequestValidatorTests
{
    [Test]
    public void IsValid_NullCreatePerson_Test()
    {
        bool result = CreatePersonRequestValidator.IsValid(null);
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void IsValid_NullOrInvalidName_Test(string? name)
    {
        bool result = CreatePersonRequestValidator.IsValid(new CreatePerson
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
        bool result = CreatePersonRequestValidator.IsValid(new CreatePerson
        {
            Name = name
        });
        Assert.That(result, Is.True);
    }
}
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
