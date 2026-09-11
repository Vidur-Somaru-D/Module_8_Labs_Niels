namespace Derivco.PlayerApi.Tests.Controllers;
// CHANGE (Task 4): Import HTTP status-code constants used by the new endpoint test.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Derivco.PlayerApi.Controllers;
using Derivco.PlayerApi.Models;
using Derivco.PlayerApi.Options;
using Derivco.PlayerApi.Services;
public class PlayersControllerTests
{
   private readonly Mock<IPlayerService> _serviceMock;
   private readonly PlayersController _controller;
   public PlayersControllerTests()
   {
       _serviceMock = new Mock<IPlayerService>();
       var options = Options.Create(new DatabaseOptions
       {
           CommandTimeoutSeconds = 30,
       });
       _controller = new PlayersController(
           _serviceMock.Object,
           options);
   }
   private static PlayerDto MakeDto(
       int id = 1,
       string username = "alice",
       decimal balance = 100m) =>
       new()
       {
           PlayerId = id,
           Username = username,
           Balance = balance,
           Region = "ZA",
       };
   // --- GetById ---
   [Fact]
   public void GetById_WhenPlayerExists_Returns200WithDto()
   {
       // Arrange
       var dto = MakeDto();
       _serviceMock
           .Setup(service => service.GetPlayer(1))
           .Returns(dto);
       // Act
       var result = _controller.GetById(1);
       // Assert
       var ok = Assert.IsType<OkObjectResult>(result);
       Assert.Equal(dto, ok.Value);
   }
   [Fact]
   public void GetById_WhenPlayerNotFound_Returns404()
   {
       // Arrange
       _serviceMock
           .Setup(service => service.GetPlayer(It.IsAny<int>()))
           .Returns((PlayerDto?)null);
       // Act
       var result = _controller.GetById(999);
       // Assert
       Assert.IsType<NotFoundResult>(result);
   }
   //GetTransactions
   //Verify that an existing player's transactions produce HTTP 200 and the expected response.
   [Fact]
   public void GetTransactions_WhenPlayerExists_Returns200WithTransactions()
   {
       // Arrange
       IReadOnlyCollection<TransactionDto> transactions =
           new List<TransactionDto>
           {
               new()
               {
                   TransactionId = 10,
                   PlayerId = 1,
                   Amount = 125.50m,
                   TransactionType = "Deposit",
                   CreatedAt = new DateTime(
                       2026,
                       9,
                       10,
                       8,
                       0,
                       0,
                       DateTimeKind.Utc),
               },
           };
       _serviceMock
           .Setup(service => service.GetPlayerTransactions(1))
           .Returns(transactions);
       // Act
       var result = _controller.GetTransactions(1);
       // Assert
       var ok = Assert.IsType<OkObjectResult>(result);
       Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
       Assert.Same(transactions, ok.Value);
   }
   // CHANGE (Task 4): Verify that an unknown player produces HTTP 404 rather than an empty HTTP 200 response.
   [Fact]
   public void GetTransactions_WhenPlayerNotFound_Returns404()
   {
       // Arrange
       _serviceMock
           .Setup(service => service.GetPlayerTransactions(999))
           .Returns((IReadOnlyCollection<TransactionDto>?)null);
       // Act
       var result = _controller.GetTransactions(999);
       // Assert
       Assert.IsType<NotFoundObjectResult>(result);
   }
   // --- Create ---
   [Fact]
   public void Create_ValidRequest_Returns201Created()
   {
       // Arrange
       var request = new CreatePlayerRequest
       {
           Username = "bob",
           InitialBalance = 50m,
           Region = "UK",
       };
       var dto = MakeDto(
           id: 7,
           username: "bob",
           balance: 50m);
       _serviceMock
           .Setup(service => service.CreatePlayer(request))
           .Returns(dto);
       // Act
       var result = _controller.Create(request);
       // Assert
       var created = Assert.IsType<CreatedAtActionResult>(result);
       Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
       Assert.Equal(dto, created.Value);
       Assert.Equal(7, created.RouteValues!["id"]);
   }
   // --- UpdateBalance ---
   [Fact]
   public void UpdateBalance_WhenPlayerExists_Returns200()
   {
       // Arrange
       var dto = MakeDto(balance: 250m);
       _serviceMock
           .Setup(service => service.UpdateBalance(1, 250m))
           .Returns(dto);
       // Act
       var result = _controller.UpdateBalance(
           1,
           new UpdateBalanceRequest
           {
               NewBalance = 250m,
           });
       // Assert
       var ok = Assert.IsType<OkObjectResult>(result);
       Assert.Equal(dto, ok.Value);
   }
   [Fact]
   public void UpdateBalance_WhenPlayerNotFound_Returns404()
   {
       // Arrange
       _serviceMock
           .Setup(service => service.UpdateBalance(
               It.IsAny<int>(),
               It.IsAny<decimal>()))
           .Returns((PlayerDto?)null);
       // Act
       var result = _controller.UpdateBalance(
           999,
           new UpdateBalanceRequest
           {
               NewBalance = 100m,
           });
       // Assert
       Assert.IsType<NotFoundResult>(result);
   }
   // --- Transfer ---
   [Fact]
   public void Transfer_OnSuccess_Returns204NoContent()
   {
       // Arrange
       _serviceMock
           .Setup(service => service.TransferFunds(
               It.IsAny<TransferFundsRequest>()));
       var request = new TransferFundsRequest
       {
           FromPlayerId = 1,
           ToPlayerId = 2,
           Amount = 50m,
       };
       // Act
       var result = _controller.Transfer(1, request);
       // Assert
       Assert.IsType<NoContentResult>(result);
   }
   [Fact]
   public void Transfer_WhenFromIdMismatch_Returns400()
   {
       // Arrange
       var request = new TransferFundsRequest
       {
           FromPlayerId = 2,
           ToPlayerId = 3,
           Amount = 50m,
       };
       // Act — route says fromId=1 but body says FromPlayerId=2
       var result = _controller.Transfer(1, request);
       // Assert
       Assert.IsType<BadRequestObjectResult>(result);
   }
   [Fact]
   public void Transfer_WhenPlayerNotFound_Returns404()
   {
       // Arrange
       _serviceMock
           .Setup(service => service.TransferFunds(
               It.IsAny<TransferFundsRequest>()))
           .Throws(new KeyNotFoundException("Player 99 not found"));
       var request = new TransferFundsRequest
       {
           FromPlayerId = 1,
           ToPlayerId = 99,
           Amount = 50m,
       };
       // Act
       var result = _controller.Transfer(1, request);
       // Assert
       Assert.IsType<NotFoundObjectResult>(result);
   }
   [Fact]
   public void Transfer_WhenInsufficientFunds_Returns400()
   {
       // Arrange
       _serviceMock
           .Setup(service => service.TransferFunds(
               It.IsAny<TransferFundsRequest>()))
           .Throws(new InvalidOperationException(
               "Insufficient funds"));
       var request = new TransferFundsRequest
       {
           FromPlayerId = 1,
           ToPlayerId = 2,
           Amount = 99999m,
       };
       // Act
       var result = _controller.Transfer(1, request);
       // Assert
       Assert.IsType<BadRequestObjectResult>(result);
   }
}