using StargateAPI.Business.Commands;
using StargateAPI.Business.Validators;

namespace apiTests.Business.Validators;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8601 // Possible null reference assignment.
public class UpdatePersonRequestValidatorTests
{
    [Test]
    public void IsValid_NullUpdatePerson_Test()
    {
        bool result =  UpdatePersonRequestValidator.IsValid(null);
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase("", "Name2")]
    [TestCase("Name1", "")]
    public void IsValid_NullOrInvalidNames_Test(string? currentName, string? newName)
    {
        bool result =  UpdatePersonRequestValidator.IsValid(new UpdatePerson
        {
            CurrentName = currentName,
            NewName = newName
        });
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase(" ", " ")]
    [TestCase(" ", "Name2")]
    [TestCase("Name1", " ")]
    [TestCase("Name1", "Name2")]
    public void IsValid_ValidNames_Test(string currentName, string newName)
    {
        bool result =  UpdatePersonRequestValidator.IsValid(new UpdatePerson
        {
            CurrentName = currentName,
            NewName = newName
        });
        Assert.That(result, Is.True);
    }
}
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
