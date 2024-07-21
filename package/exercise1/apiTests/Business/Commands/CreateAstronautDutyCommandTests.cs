using Microsoft.AspNetCore.Http;
using Moq;
using StargateAPI;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Repositories;

namespace apiTests.Business.Commands;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
public class CreateAstronautDutyCommandTests
{
    private CreateAstronautDutyPreProcessor _preprocessor;
    private CreateAstronautDutyHandler _handler;

    private Mock<IStargateRepository> _stargateRepositoryMock;

    [SetUp]
    public void Setup()
    {
        _stargateRepositoryMock = new Mock<IStargateRepository>();

        _preprocessor = new CreateAstronautDutyPreProcessor(_stargateRepositoryMock.Object, new AppConfig());
        _handler = new CreateAstronautDutyHandler(_stargateRepositoryMock.Object, new AppConfig());
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

        CreateAstronautDuty request = new()
        {
            Name = "Its Me",
            Rank = "Seargent",
            DutyTitle = "King of the Hill",
            DutyStartDate = DateTime.Now.AddYears(-7),
        };
        Assert.ThrowsAsync<BadHttpRequestException>(() => _preprocessor.Process(request, CancellationToken.None));
    }

    [Test]
    public void Process_PersonFound_PreviousDutyFound_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetUntrackedAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Person
            {
                Id = 1,
                Name = "Its Me"
            });
        _stargateRepositoryMock
            .Setup(x => x.GetAstronautDutyByIdTitleStartDateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AstronautDuty
            {
                Id = 1,
                PersonId = 1,
                Rank = "Seargent",
                DutyTitle = "King of the Hill",
                DutyStartDate = DateTime.Now.AddYears(-7),
                DutyEndDate = null
            });

        CreateAstronautDuty request = new()
        {
            Name = "Its Me",
            Rank = "Seargent",
            DutyTitle = "King of the Hill",
            DutyStartDate = DateTime.Now.AddYears(-7),
        };
        Assert.ThrowsAsync<BadHttpRequestException>(() => _preprocessor.Process(request, CancellationToken.None));
    }

    [Test]
    public void Process_PersonFound_PreviousDutyNotFound_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetUntrackedAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Person
            {
                Id = 1,
                Name = "Its Me"
            });
        _stargateRepositoryMock
            .Setup(x => x.GetAstronautDutyByIdTitleStartDateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDuty)null);

        CreateAstronautDuty request = new()
        {
            Name = "Its Me",
            Rank = "Seargent",
            DutyTitle = "King of the Hill",
            DutyStartDate = DateTime.Now.AddYears(-7),
        };
        Task result = _preprocessor.Process(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
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
            .Setup(x => x.GetUntrackedAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person)null);

        CreateAstronautDuty request = new()
        {
            Name = "Its Me",
            Rank = "Seargent",
            DutyTitle = "King of the Hill",
            DutyStartDate = DateTime.Now.AddYears(-7),
        };
        CreateAstronautDutyResult result = await _handler.Handle(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ResponseCode, Is.EqualTo(404));
            Assert.That(result.Message, Is.EqualTo("No person was found matching name 'Its Me'"));
        });
    }

    [Test]
    public async Task Handle_PersonFound_DetailNotFound_DutyNotFound_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetUntrackedAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Person
            {
                Id = 1,
                Name = "Its Me"
            });
        _stargateRepositoryMock
            .Setup(x => x.GetAstronautDetailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDetail)null);
        _stargateRepositoryMock
            .Setup(x => x.GetMostRecentAstronautDutyByAstronautIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDuty)null);

        CreateAstronautDuty request = new()
        {
            Name = "Its Me",
            Rank = "Seargent",
            DutyTitle = "King of the Hill",
            DutyStartDate = DateTime.Now.AddYears(-7),
        };
        CreateAstronautDutyResult result = await _handler.Handle(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.ResponseCode, Is.EqualTo(200));
        });
        _stargateRepositoryMock.Verify(
            x => x.AddAstronautDetailAsync(It.IsAny<AstronautDetail>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _stargateRepositoryMock.Verify(
            x => x.UpdateAstronautDetailAsync(It.IsAny<AstronautDetail>(), It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _stargateRepositoryMock.Verify(
            x => x.UpdateAstronautDutyAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _stargateRepositoryMock.Verify(
            x => x.AddAstronautDutyAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_PersonFound_DetailNotFound_DutyNotFound_Retired_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetUntrackedAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Person
            {
                Id = 1,
                Name = "Its Me"
            });
        _stargateRepositoryMock
            .Setup(x => x.GetAstronautDetailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDetail)null);
        _stargateRepositoryMock
            .Setup(x => x.GetMostRecentAstronautDutyByAstronautIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AstronautDuty)null);

        CreateAstronautDuty request = new()
        {
            Name = "Its Me",
            Rank = "Seargent",
            DutyTitle = "RETIRED",
            DutyStartDate = DateTime.Now.AddYears(-7),
        };
        CreateAstronautDutyResult result = await _handler.Handle(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.ResponseCode, Is.EqualTo(200));
        });
        _stargateRepositoryMock.Verify(
            x => x.AddAstronautDetailAsync(It.IsAny<AstronautDetail>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _stargateRepositoryMock.Verify(
            x => x.UpdateAstronautDetailAsync(It.IsAny<AstronautDetail>(), It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _stargateRepositoryMock.Verify(
            x => x.UpdateAstronautDutyAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _stargateRepositoryMock.Verify(
            x => x.AddAstronautDutyAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_PersonFound_DetailFound_DutyFound_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetUntrackedAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Person
            {
                Id = 1,
                Name = "Its Me"
            });
        _stargateRepositoryMock
            .Setup(x => x.GetAstronautDetailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AstronautDetail
            {
                Id = 1,
                PersonId = 1,
                CurrentRank = "Seargent",
                CurrentDutyTitle = "King of the Hill",
                CareerStartDate = DateTime.Now.AddYears(-17),
                CareerEndDate = null
            });
        _stargateRepositoryMock
            .Setup(x => x.GetMostRecentAstronautDutyByAstronautIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AstronautDuty
            {
                Id = 1,
                PersonId = 1,
                Rank = "Seargent",
                DutyTitle = "King of the Hill",
                DutyStartDate = DateTime.Now.AddYears(-7),
                DutyEndDate = null
            });

        CreateAstronautDuty request = new()
        {
            Name = "Its Me",
            Rank = "Seargent",
            DutyTitle = "King of the Hill",
            DutyStartDate = DateTime.Now.AddYears(-7),
        };
        CreateAstronautDutyResult result = await _handler.Handle(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.ResponseCode, Is.EqualTo(200));
        });
        _stargateRepositoryMock.Verify(
            x => x.AddAstronautDetailAsync(It.IsAny<AstronautDetail>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _stargateRepositoryMock.Verify(
            x => x.UpdateAstronautDetailAsync(It.IsAny<AstronautDetail>(), It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _stargateRepositoryMock.Verify(
            x => x.UpdateAstronautDutyAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _stargateRepositoryMock.Verify(
            x => x.AddAstronautDutyAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_PersonFound_DetailFound_DutyFound_Retired_Test()
    {
        _stargateRepositoryMock
            .Setup(x => x.GetUntrackedAstronautByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Person
            {
                Id = 1,
                Name = "Its Me"
            });
        _stargateRepositoryMock
            .Setup(x => x.GetAstronautDetailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AstronautDetail
            {
                Id = 1,
                PersonId = 1,
                CurrentRank = "Seargent",
                CurrentDutyTitle = "King of the Hill",
                CareerStartDate = DateTime.Now.AddYears(-17),
                CareerEndDate = null
            });
        _stargateRepositoryMock
            .Setup(x => x.GetMostRecentAstronautDutyByAstronautIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AstronautDuty
            {
                Id = 1,
                PersonId = 1,
                Rank = "Seargent",
                DutyTitle = "King of the Hill",
                DutyStartDate = DateTime.Now.AddYears(-7),
                DutyEndDate = null
            });

        CreateAstronautDuty request = new()
        {
            Name = "Its Me",
            Rank = "Seargent",
            DutyTitle = "RETIRED",
            DutyStartDate = DateTime.Now.AddYears(-7),
        };
        CreateAstronautDutyResult result = await _handler.Handle(request, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.ResponseCode, Is.EqualTo(200));
        });
        _stargateRepositoryMock.Verify(
            x => x.AddAstronautDetailAsync(It.IsAny<AstronautDetail>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _stargateRepositoryMock.Verify(
            x => x.UpdateAstronautDetailAsync(It.IsAny<AstronautDetail>(), It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _stargateRepositoryMock.Verify(
            x => x.UpdateAstronautDutyAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _stargateRepositoryMock.Verify(
            x => x.AddAstronautDutyAsync(It.IsAny<AstronautDuty>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
