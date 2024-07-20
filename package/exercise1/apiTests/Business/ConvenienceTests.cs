using StargateAPI.Business;
using StargateAPI.Business.Data;

namespace apiTests.Business;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8604 // Possible null reference argument.
public class ConvenienceTests
{
    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void IsValid_NullOrInvalidName_Test(string? name)
    {
        Person? result = Convenience.GetUntrackedPersonByName(null, name);
        Assert.That(result, Is.Null);
    }
}
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
