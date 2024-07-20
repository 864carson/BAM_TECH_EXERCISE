using Moq;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Queries;
using StargateAPI.Repositories;

namespace apiTests.Business.Queries;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
public class GetPersonByNameQueriesTests
{
    private GetPersonByNameHandler _handler;

    private Mock<IStargateRepository> _personRepositoryMock;

    [SetUp]
    public void Setup()
    {
        _personRepositoryMock = new Mock<IStargateRepository>();

        _handler = new GetPersonByNameHandler(_personRepositoryMock.Object);
    }

    [Test]
    public void Handle_InvalidRequest_Test()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(null, CancellationToken.None));
    }

    [Test]
    public async Task Handle_PersonNotFound_Test()
    {
        _personRepositoryMock
            .Setup(x => x.GetAstronautDetailsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonAstronaut)null);

        GetPersonByName request = new() { Name = "Who" };
        GetPersonByNameResult result = await _handler.Handle(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Person, Is.Null);
    }

    [Test]
    public async Task Handle_PersonFound_Test()
    {
        _personRepositoryMock
            .Setup(x => x.GetAstronautDetailsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonAstronaut
            {
                PersonId = 1,
                Name = "Tester",
                CurrentRank = "General",
                CurrentDutyTitle = "Master of the Universe",
                CareerStartDate = DateTime.Now.AddYears(-30),
                CareerEndDate = DateTime.Now
            });

        GetPersonByName request = new() { Name = "Tester" };
        GetPersonByNameResult result = await _handler.Handle(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Person, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Person.PersonId, Is.EqualTo(1));
            Assert.That(result.Person.Name, Is.EqualTo("Tester"));
            Assert.That(result.Person.CurrentRank, Is.EqualTo("General"));
            Assert.That(result.Person.CurrentDutyTitle, Is.EqualTo("Master of the Universe"));
        });
    }
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
