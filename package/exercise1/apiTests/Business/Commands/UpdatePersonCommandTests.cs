using Microsoft.AspNetCore.Http;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Repositories;

namespace apiTests.Business.Commands;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
public class UpdatePersonCommandTests
{
    private UpdatePersonPreProcessor _preprocessor;
    private UpdatePersonHandler _handler;

    private Mock<IStargateRepository> _stargateRepositoryMock;

    [SetUp]
    public void Setup()
    {
        _stargateRepositoryMock = new Mock<IStargateRepository>();

        _preprocessor = new UpdatePersonPreProcessor(_stargateRepositoryMock.Object);
        _handler = new UpdatePersonHandler(_stargateRepositoryMock.Object);
    }

    #region [ RequestPreProcessor]

    [Test]
    public void Process_InvalidRequest_Test()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _preprocessor.Process(null, CancellationToken.None));
    }

    [Test]
    public void Process_PersonNotFound_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetUntrackedAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person)null);

        UpdatePerson request = new() { CurrentName = "OldMe", NewName = "NewMe" };
        Assert.ThrowsAsync<BadHttpRequestException>(() => _preprocessor.Process(request, CancellationToken.None));
    }

    [Test]
    public void Process_PersonFound_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetUntrackedAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Person
            {
                Id = 1,
                Name = "Tester"
            });

        UpdatePerson request = new() { CurrentName = "OldMe", NewName = "NewMe" };
        Task result = _preprocessor.Process(request, CancellationToken.None);

        Assert.That(result.IsCompleted, Is.True);
        Assert.That(result.IsFaulted, Is.False);
    }

    #endregion

    #region [ RequestHandler ]

    [Test]
    public void Handle_InvalidRequest_Test()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(null, CancellationToken.None));
    }

    [Test]
    public async Task Handle_PersonNotFound_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person)null);

        UpdatePerson request = new() { CurrentName = "OldMe", NewName = "NewMe" };
        UpdatePersonResult result = await _handler.Handle(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.ResponseCode, Is.EqualTo(404));
        Assert.That(result.Message, Is.EqualTo("No person was found matching name 'OldMe'"));
    }

    [Test]
    public async Task Handle_PersonFound_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Person
            {
                Id = 1,
                Name = "Tester"                
            });
        _stargateRepositoryMock
            .Setup(x => x.UpdateAstronautNameAsync(It.IsAny<Person>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        UpdatePerson request = new() { CurrentName = "OldMe", NewName = "NewMe" };
        UpdatePersonResult result = await _handler.Handle(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
    }

    #endregion

}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
