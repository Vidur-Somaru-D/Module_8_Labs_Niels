namespace Derivco.PlayerApi.Tests.Services;

using Moq;
using Xunit;
using Derivco.PlayerApi.Models;
using Derivco.PlayerApi.Repositories;
using Derivco.PlayerApi.Services;

public class PlayerServiceTests
{
    private readonly Mock<IPlayerRepository> _repositoryMock;
    private readonly PlayerService _service;

    public PlayerServiceTests()
    {
        _repositoryMock = new Mock<IPlayerRepository>();
        _service = new PlayerService(_repositoryMock.Object);
    }

    [Fact]
    public void GetPlayer_WhenPlayerExists_ReturnsDto()
    {
        // Arrange
        var player = new Player { PlayerId = 1, Username = "alice", Balance = 100m, Region = "ZA", IsActive = true };
        _repositoryMock.Setup(r => r.GetById(1)).Returns(player);

        // Act
        var result = _service.GetPlayer(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PlayerId);
        Assert.Equal("alice", result.Username);
        Assert.Equal(100m, result.Balance);
    }

    [Fact]
    public void GetPlayer_WhenPlayerNotFound_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Player?)null);

        // Act
        var result = _service.GetPlayer(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CreatePlayer_ValidRequest_ReturnsDtoWithCorrectFields()
    {
        // Arrange
        var request = new CreatePlayerRequest { Username = "bob", InitialBalance = 50m, Region = "UK" };
        var created = new Player { PlayerId = 99, Username = "bob", Balance = 50m, Region = "UK", IsActive = true };
        _repositoryMock.Setup(r => r.Create(It.IsAny<Player>())).Returns(created);

        // Act
        var result = _service.CreatePlayer(request);

        // Assert
        Assert.Equal(99, result.PlayerId);
        Assert.Equal("bob", result.Username);
        Assert.Equal(50m, result.Balance);
        _repositoryMock.Verify(r => r.Create(It.IsAny<Player>()), Times.Once);
    }

    [Fact]
    public void TransferFunds_WhenFromPlayerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetById(1)).Returns((Player?)null);

        var request = new TransferFundsRequest { FromPlayerId = 1, ToPlayerId = 2, Amount = 50m };

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => _service.TransferFunds(request));
        _repositoryMock.Verify(r => r.TransferFunds(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public void TransferFunds_WhenToPlayerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var from = new Player { PlayerId = 1, Username = "alice", Balance = 200m, Region = "ZA", IsActive = true };
        _repositoryMock.Setup(r => r.GetById(1)).Returns(from);
        _repositoryMock.Setup(r => r.GetById(2)).Returns((Player?)null);

        var request = new TransferFundsRequest { FromPlayerId = 1, ToPlayerId = 2, Amount = 50m };

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => _service.TransferFunds(request));
        _repositoryMock.Verify(r => r.TransferFunds(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public void TransferFunds_WhenRepositoryThrowsInsufficientFunds_ExceptionPropagates()
    {
        // Arrange
        var from = new Player { PlayerId = 1, Username = "alice", Balance = 10m, Region = "ZA", IsActive = true };
        var to   = new Player { PlayerId = 2, Username = "bob",   Balance = 0m,  Region = "UK", IsActive = true };
        _repositoryMock.Setup(r => r.GetById(1)).Returns(from);
        _repositoryMock.Setup(r => r.GetById(2)).Returns(to);
        _repositoryMock
            .Setup(r => r.TransferFunds(1, 2, 50m))
            .Throws(new InvalidOperationException("Insufficient funds"));

        var request = new TransferFundsRequest { FromPlayerId = 1, ToPlayerId = 2, Amount = 50m };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.TransferFunds(request));
    }

    [Theory]
    [InlineData(100.00, 99.99, true)]   // just enough
    [InlineData(100.00, 100.00, true)]  // exactly the balance
    [InlineData(100.00, 0.01, true)]    // minimal transfer
    public void TransferFunds_WithValidAmounts_CallsRepository(decimal balance, decimal transferAmount, bool shouldCallRepo)
    {
        // Arrange
        var from = new Player { PlayerId = 1, Balance = balance, IsActive = true, Username = "a", Region = "ZA" };
        var to   = new Player { PlayerId = 2, Balance = 0m,      IsActive = true, Username = "b", Region = "ZA" };
        _repositoryMock.Setup(r => r.GetById(1)).Returns(from);
        _repositoryMock.Setup(r => r.GetById(2)).Returns(to);

        var request = new TransferFundsRequest { FromPlayerId = 1, ToPlayerId = 2, Amount = transferAmount };

        // Act
        _service.TransferFunds(request);

        // Assert
        _repositoryMock.Verify(
            r => r.TransferFunds(1, 2, transferAmount),
            shouldCallRepo ? Times.Once() : Times.Never());
    }
}
