using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using JaCore.Api.Services.Device;
using JaCore.Api.Services.Repositories.Device;
using JaCore.Api.Data;
using JaCore.Api.DTOs.Device;
using JaCore.Api.Entities.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JaCore.Common.Device; // For enums

namespace JaCore.Api.UnitTests.Services.Device;

public class DeviceCardServiceTests
{
    private readonly Mock<IDeviceCardRepository> _mockCardRepo;
    private readonly Mock<IDeviceRepository> _mockDeviceRepo; // For checking device existence
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ILogger<DeviceCardService>> _mockLogger;
    private readonly DeviceCardService _sut;

    public DeviceCardServiceTests()
    {
        _mockCardRepo = new Mock<IDeviceCardRepository>();
        _mockDeviceRepo = new Mock<IDeviceRepository>();
        _mockLogger = new Mock<ILogger<DeviceCardService>>();

        var options = new DbContextOptions<ApplicationDbContext>();
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1); // Default success for SaveChanges

        _sut = new DeviceCardService(
            _mockCardRepo.Object,
            _mockDeviceRepo.Object,
            _mockDbContext.Object,
            _mockLogger.Object
        );
    }

    private void VerifyLog(LogLevel level, string messageContains, Times times)
    {
         _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageContains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            times);
    }

    // Add a specific overload for VerifyLog<TException>
    private void VerifyLog<TException>(LogLevel level, string messageContains, Times times) where TException : Exception
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageContains)),
                It.Is<TException>(ex => true), // Check exception type
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!), // Keep matcher flexible for message
            times);
    }

    #region CreateDeviceCardAsync Tests

    [Fact]
    public async Task CreateDeviceCardAsync_WhenDeviceExistsAndCardDoesnt_ShouldReturnCreatedDto()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var createDto = new DeviceCardCreateDto(deviceId, "Location A", "User1", "{}");
        var cardId = Guid.NewGuid();

        _mockDeviceRepo.Setup(r => r.ExistsAsync(deviceId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockCardRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false); // Card doesn't exist for device
        _mockCardRepo.Setup(r => r.AddAsync(It.IsAny<DeviceCard>(), It.IsAny<CancellationToken>()))
                     .Callback<DeviceCard, CancellationToken>((card, ct) => card.Id = cardId); // Assign ID
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateDeviceCardAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cardId);
        result.DeviceId.Should().Be(deviceId);
        result.Location.Should().Be(createDto.Location);

        _mockDeviceRepo.Verify(r => r.ExistsAsync(deviceId, It.IsAny<CancellationToken>()), Times.Once);
        _mockCardRepo.Verify(r => r.ExistsAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCardRepo.Verify(r => r.AddAsync(It.Is<DeviceCard>(c => c.DeviceId == deviceId), It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDeviceCardAsync_WhenDeviceDoesNotExist_ShouldReturnNullAndLogWarning()
    {
        // Arrange
        var nonExistentDeviceId = Guid.NewGuid();
        var createDto = new DeviceCardCreateDto(nonExistentDeviceId, "Location B", null, null);

        _mockDeviceRepo.Setup(r => r.ExistsAsync(nonExistentDeviceId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _sut.CreateDeviceCardAsync(createDto);

        // Assert
        result.Should().BeNull();
        VerifyLog(LogLevel.Warning, "DeviceCard creation failed: Device with ID", Times.Once());
        _mockCardRepo.Verify(r => r.AddAsync(It.IsAny<DeviceCard>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateDeviceCardAsync_WhenCardAlreadyExistsForDevice_ShouldReturnNullAndLogWarning()
    {
         // Arrange
        var deviceId = Guid.NewGuid();
        var createDto = new DeviceCardCreateDto(deviceId, "Location A", "User1", "{}");

        _mockDeviceRepo.Setup(r => r.ExistsAsync(deviceId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockCardRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true); // Card *does* exist for device

        // Act
        var result = await _sut.CreateDeviceCardAsync(createDto);

        // Assert
        result.Should().BeNull();
        VerifyLog(LogLevel.Warning, "card already exists for Device ID", Times.Once());
        _mockCardRepo.Verify(r => r.AddAsync(It.IsAny<DeviceCard>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateDeviceCardAsync_WhenSaveChangesFails_ShouldReturnNullAndLogError()
    {
        // Arrange
         var deviceId = Guid.NewGuid();
        var createDto = new DeviceCardCreateDto(deviceId, "Location A", "User1", "{}");

        _mockDeviceRepo.Setup(r => r.ExistsAsync(deviceId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockCardRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new DbUpdateException("Simulated DB error")); // Throw exception instead of returning 0

        // Act
        var result = await _sut.CreateDeviceCardAsync(createDto);

        // Assert
        result.Should().BeNull();
        // Verify the generic error log from BaseDeviceService.SaveChangesAsync
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error saving changes to the database.")),
                It.IsAny<DbUpdateException>(), // Expect DbUpdateException
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once());
        _mockCardRepo.Verify(r => r.AddAsync(It.IsAny<DeviceCard>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDeviceCardAsync_WhenDeviceRepoExistsThrows_ShouldReturnNullAndLogError()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var createDto = new DeviceCardCreateDto(deviceId, "Loc", null, null);
        var exception = new Exception("DB error checking device");
        _mockDeviceRepo.Setup(r => r.ExistsAsync(deviceId, It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        // Act
        var result = await _sut.CreateDeviceCardAsync(createDto);

        // Assert
        result.Should().BeNull();
        // Verify the log from the generic catch block in CreateDeviceCardAsync
        VerifyLog(LogLevel.Error, "Error during DeviceCard creation", Times.Once());
        _mockCardRepo.Verify(r => r.AddAsync(It.IsAny<DeviceCard>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateDeviceCardAsync_WhenCardRepoExistsThrows_ShouldReturnNullAndLogError()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var createDto = new DeviceCardCreateDto(deviceId, "Loc", null, null);
         var exception = new Exception("DB error checking card");

        _mockDeviceRepo.Setup(r => r.ExistsAsync(deviceId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
         _mockCardRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(exception);

        // Act
        var result = await _sut.CreateDeviceCardAsync(createDto);

        // Assert
        result.Should().BeNull();
         // Verify the log from the generic catch block in CreateDeviceCardAsync
        VerifyLog(LogLevel.Error, "Error during DeviceCard creation", Times.Once());
        _mockCardRepo.Verify(r => r.AddAsync(It.IsAny<DeviceCard>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateDeviceCardAsync_WhenCardRepoAddThrows_ShouldReturnNullAndLogError()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var createDto = new DeviceCardCreateDto(deviceId, "Loc", null, null);
        var exception = new Exception("DB error adding card");

        _mockDeviceRepo.Setup(r => r.ExistsAsync(deviceId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockCardRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);
        _mockCardRepo.Setup(r => r.AddAsync(It.IsAny<DeviceCard>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(exception);

        // Act
        var result = await _sut.CreateDeviceCardAsync(createDto);

        // Assert
        result.Should().BeNull();
        // Verify the log from the generic catch block in CreateDeviceCardAsync
        VerifyLog(LogLevel.Error, "Error during DeviceCard creation", Times.Once());
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    // Add tests for Get*, Update*, Delete* methods similarly...

    #region GetDeviceCardByIdAsync Tests

    [Fact]
    public async Task GetDeviceCardByIdAsync_WhenCardExists_ShouldReturnDto()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        // Use correct states from JaCore.Common.Device
        var cardEntity = new DeviceCard { Id = cardId, DeviceId = deviceId, Location = "TestLoc", OperationalState = DeviceOperationalState.Active };
        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(cardEntity);

        // Act
        var result = await _sut.GetDeviceCardByIdAsync(cardId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cardId);
        result.Location.Should().Be(cardEntity.Location);
        _mockCardRepo.Verify(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDeviceCardByIdAsync_WhenCardDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockCardRepo.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>())).ReturnsAsync((DeviceCard?)null);

        // Act
        var result = await _sut.GetDeviceCardByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDeviceCardByIdAsync_WhenRepositoryThrows_ShouldReturnNullAndLogError()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var exception = new TimeoutException("Timeout");
        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        // Act
        var result = await _sut.GetDeviceCardByIdAsync(cardId);

        // Assert
        result.Should().BeNull();
        VerifyLog(LogLevel.Error, $"Error retrieving device card with ID {cardId}", Times.Once());
    }

    #endregion

    #region GetDeviceCardsByDeviceIdAsync Tests

    [Fact]
    public async Task GetDeviceCardsByDeviceIdAsync_WhenCardsExistForDevice_ShouldReturnDtos()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var cards = new List<DeviceCard>
        {
            new DeviceCard { Id = Guid.NewGuid(), DeviceId = deviceId, Location = "Loc1" },
            new DeviceCard { Id = Guid.NewGuid(), DeviceId = deviceId, Location = "Loc2" }
        };
        _mockCardRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(cards);

        // Act
        var result = await _sut.GetDeviceCardsByDeviceIdAsync(deviceId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Location.Should().Be("Loc1");
        // Use It.IsAny for the expression verification for simplicity
        _mockCardRepo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDeviceCardsByDeviceIdAsync_WhenNoCardsExistForDevice_ShouldReturnEmptyList()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        _mockCardRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<DeviceCard>());

        // Act
        var result = await _sut.GetDeviceCardsByDeviceIdAsync(deviceId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDeviceCardsByDeviceIdAsync_WhenRepositoryThrows_ShouldReturnEmptyListAndLogError()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var exception = new Exception("Query failed");
        _mockCardRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(exception);

        // Act
        var result = await _sut.GetDeviceCardsByDeviceIdAsync(deviceId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        VerifyLog(LogLevel.Error, $"Error retrieving device cards for device ID {deviceId}", Times.Once());
    }

    #endregion

    #region UpdateDeviceCardAsync Tests

    [Fact]
    public async Task UpdateDeviceCardAsync_WhenCardExistsAndDeviceExists_ShouldReturnTrue()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        // Update DTO only has Location, AssignedUser, PropertiesJson
        var updateDto = new DeviceCardUpdateDto("New Location", "User2", "New Config");
        var existingCard = new DeviceCard { Id = cardId, DeviceId = deviceId, Location = "Old Location" };

        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCard);
        // Assume device still exists (no need to mock DeviceRepo for happy path update if DeviceId doesn't change)
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateDeviceCardAsync(cardId, updateDto);

        // Assert
        result.Should().BeTrue();
        _mockCardRepo.Verify(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()), Times.Once);
        _mockCardRepo.Verify(r => r.Update(It.Is<DeviceCard>(c =>
            c.Id == cardId &&
            c.Location == updateDto.Location &&
            c.AssignedUser == updateDto.AssignedUser && // Check correct fields
            c.PropertiesJson == updateDto.PropertiesJson
            // Don't check Status, LastUpdatedBy, Notes as they don't exist
        )), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDeviceCardAsync_WhenCardDoesNotExist_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        // Use correct constructor
        var updateDto = new DeviceCardUpdateDto("Loc", null, null);
        _mockCardRepo.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>())).ReturnsAsync((DeviceCard?)null);

        // Act
        var result = await _sut.UpdateDeviceCardAsync(nonExistentId, updateDto);

        // Assert
        result.Should().BeFalse();
        // Verify the specific warning log from the UpdateDeviceCardAsync method
        VerifyLog(LogLevel.Warning, "DeviceCard update failed: Card not found", Times.Once());
        _mockCardRepo.Verify(r => r.Update(It.IsAny<DeviceCard>()), Times.Never);
    }

     [Fact]
    public async Task UpdateDeviceCardAsync_WhenSaveChangesFails_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        // Use correct constructor
        var updateDto = new DeviceCardUpdateDto("New Location", "User2", "New Config");
        var existingCard = new DeviceCard { Id = cardId, DeviceId = Guid.NewGuid(), Location = "Old Location" };

        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCard);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new DbUpdateException("Simulated DB error")); // Simulate save failure

        // Act
        var result = await _sut.UpdateDeviceCardAsync(cardId, updateDto);

        // Assert
        result.Should().BeFalse();
        // Verify the generic error log from BaseDeviceService.SaveChangesAsync
        VerifyLog<DbUpdateException>(LogLevel.Error, "Error saving changes to the database.", Times.Once());
        _mockCardRepo.Verify(r => r.Update(It.IsAny<DeviceCard>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDeviceCardAsync_WhenConcurrencyIssueOccurs_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        // Use correct constructor
        var updateDto = new DeviceCardUpdateDto("Loc", null, null);
        var existingCard = new DeviceCard { Id = cardId, DeviceId = Guid.NewGuid() };
        var exception = new DbUpdateConcurrencyException("Concurrency fail");

        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCard);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        // Act
        var result = await _sut.UpdateDeviceCardAsync(cardId, updateDto);

        // Assert
        result.Should().BeFalse();
        // Verify the generic Error log from BaseDeviceService.SaveChangesAsync
        // The specific catch block in UpdateDeviceCardAsync now only returns false, doesn't log.
        VerifyLog<DbUpdateConcurrencyException>(LogLevel.Error, "Error saving changes to the database.", Times.Once());
    }

    #endregion

    #region DeleteDeviceCardAsync Tests

    [Fact]
    public async Task DeleteDeviceCardAsync_WhenCardExists_ShouldReturnTrue()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var existingCard = new DeviceCard { Id = cardId, DeviceId = Guid.NewGuid() };
        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCard);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteDeviceCardAsync(cardId);

        // Assert
        result.Should().BeTrue();
        _mockCardRepo.Verify(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()), Times.Once);
        _mockCardRepo.Verify(r => r.Remove(existingCard), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDeviceCardAsync_WhenCardDoesNotExist_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockCardRepo.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>())).ReturnsAsync((DeviceCard?)null);

        // Act
        var result = await _sut.DeleteDeviceCardAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
        // Verify the specific warning log from the DeleteDeviceCardAsync method
        VerifyLog(LogLevel.Warning, "DeviceCard deletion failed: Card not found", Times.Once());
        _mockCardRepo.Verify(r => r.Remove(It.IsAny<DeviceCard>()), Times.Never);
    }

    [Fact]
    public async Task DeleteDeviceCardAsync_WhenSaveChangesFails_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var existingCard = new DeviceCard { Id = cardId, DeviceId = Guid.NewGuid() };
        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCard);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new DbUpdateException("Simulated DB error")); // Simulate save failure

        // Act
        var result = await _sut.DeleteDeviceCardAsync(cardId);

        // Assert
        result.Should().BeFalse();
         // Verify the generic error log from BaseDeviceService.SaveChangesAsync
        VerifyLog(LogLevel.Error, "Error saving changes to the database.", Times.Once());
        _mockCardRepo.Verify(r => r.Remove(existingCard), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDeviceCardAsync_WhenConcurrencyIssueOccurs_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var existingCard = new DeviceCard { Id = cardId, DeviceId = Guid.NewGuid() };
        var exception = new DbUpdateConcurrencyException("Concurrency fail on delete");

        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCard);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        // Act
        var result = await _sut.DeleteDeviceCardAsync(cardId);

        // Assert
        result.Should().BeFalse();
        // Call VerifyLog with the correct exception type argument
        VerifyLog<DbUpdateConcurrencyException>(LogLevel.Error, "Error saving changes to the database.", Times.Once());
    }

     [Fact]
    public async Task DeleteDeviceCardAsync_WhenDbUpdateExceptionOccurs_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var existingCard = new DeviceCard { Id = cardId, DeviceId = Guid.NewGuid() };
        // Simulate FK constraint (e.g., Event or Operation depends on this card)
        var exception = new DbUpdateException("FK constraint", new Exception());

        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCard);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        // Act
        var result = await _sut.DeleteDeviceCardAsync(cardId);

        // Assert
        result.Should().BeFalse();
        // Verify the generic error log from BaseDeviceService.SaveChangesAsync
        VerifyLog<DbUpdateException>(LogLevel.Error, "Error saving changes to the database.", Times.Once());
    }

    #endregion

} 