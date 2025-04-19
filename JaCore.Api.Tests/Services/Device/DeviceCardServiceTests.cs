using Xunit;
using Moq;
using JaCore.Api.Services.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Models.Device;
using Microsoft.Extensions.Logging;
using System; // For ArgumentException, InvalidOperationException
using System.Threading.Tasks;
using JaCore.Api.Interfaces.Services;
using DeviceModel = JaCore.Api.Models.Device.Device; // Alias for parent device

namespace JaCore.Api.Tests.Services.Device;

public class DeviceCardServiceTests
{
    private readonly Mock<IDeviceCardRepository> _mockCardRepo;
    private readonly Mock<IDeviceRepository> _mockDeviceRepo;
    private readonly Mock<ILogger<DeviceCardService>> _mockLogger;
    private readonly DeviceCardService _service;

    public DeviceCardServiceTests()
    {
        _mockCardRepo = new Mock<IDeviceCardRepository>();
        _mockDeviceRepo = new Mock<IDeviceRepository>();
        _mockLogger = new Mock<ILogger<DeviceCardService>>();
        _service = new DeviceCardService(_mockCardRepo.Object, _mockDeviceRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByDeviceIdAsync_CardExists_ReturnsDto()
    {
        // Arrange
        var deviceId = 1;
        var card = new DeviceCard { Id = 10, SerialNumber = "SN001", Device = new DeviceModel { Id = deviceId } };
        _mockCardRepo.Setup(r => r.GetByDeviceIdAsync(deviceId)).ReturnsAsync(card);

        // Act
        var result = await _service.GetByDeviceIdAsync(deviceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(card.Id, result.Id);
        Assert.Equal(card.SerialNumber, result.SerialNumber);
        _mockCardRepo.Verify(r => r.GetByDeviceIdAsync(deviceId), Times.Once);
    }

    [Fact]
    public async Task GetByDeviceIdAsync_CardDoesNotExist_ReturnsNull()
    {
        // Arrange
        var deviceId = 99;
        _mockCardRepo.Setup(r => r.GetByDeviceIdAsync(deviceId)).ReturnsAsync((DeviceCard?)null);

        // Act
        var result = await _service.GetByDeviceIdAsync(deviceId);

        // Assert
        Assert.Null(result);
        _mockCardRepo.Verify(r => r.GetByDeviceIdAsync(deviceId), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_CardExists_ReturnsDto()
    {
        // Arrange
        var cardId = 10;
        var card = new DeviceCard { Id = cardId, SerialNumber = "SN001" };
        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId)).ReturnsAsync(card);

        // Act
        var result = await _service.GetByIdAsync(cardId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(card.Id, result.Id);
        _mockCardRepo.Verify(r => r.GetByIdAsync(cardId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_CardDoesNotExist_ReturnsNull()
    {
        // Arrange
        var cardId = 99;
        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId)).ReturnsAsync((DeviceCard?)null);

        // Act
        var result = await _service.GetByIdAsync(cardId);

        // Assert
        Assert.Null(result);
         _mockCardRepo.Verify(r => r.GetByIdAsync(cardId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DeviceExists_NoExistingCard_ReturnsCreatedDto()
    {
        // Arrange
        var deviceId = 1;
        var createDto = new CreateDeviceCardDto { SerialNumber = "NEW_SN" };
        var device = new DeviceModel { Id = deviceId, DeviceCardId = null }; // Device exists, no card yet
        var addedCard = new DeviceCard { Id = 15 }; // Card after being added

        _mockDeviceRepo.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync(device);
        _mockCardRepo.Setup(r => r.AddAsync(It.IsAny<DeviceCard>()))
                     .Callback<DeviceCard>(c => { 
                         // Simulate DB assigning ID and properties being mapped
                         c.Id = addedCard.Id; 
                         c.SerialNumber = createDto.SerialNumber;
                         c.Device = device; // Link back
                         addedCard = c; // Capture the state as it would be after add
                     })
                     .ReturnsAsync(() => addedCard);
        _mockDeviceRepo.Setup(r => r.UpdateAsync(It.IsAny<DeviceModel>())).ReturnsAsync((DeviceModel d) => d); // Simulate successful device update

        // Act
        var result = await _service.CreateAsync(deviceId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedCard.Id, result.Id);
        Assert.Equal(createDto.SerialNumber, result.SerialNumber);
        _mockDeviceRepo.Verify(r => r.GetByIdAsync(deviceId), Times.Once);
        _mockCardRepo.Verify(r => r.AddAsync(It.Is<DeviceCard>(c => c.SerialNumber == createDto.SerialNumber && c.Device == device)), Times.Once);
        // Verify the device was updated to link the new card ID
        _mockDeviceRepo.Verify(r => r.UpdateAsync(It.Is<DeviceModel>(d => d.Id == deviceId && d.DeviceCardId == addedCard.Id)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DeviceNotFound_ThrowsArgumentException()
    {
        // Arrange
        var deviceId = 99;
        var createDto = new CreateDeviceCardDto { SerialNumber = "NEW_SN" };
        _mockDeviceRepo.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync((DeviceModel?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(deviceId, createDto));
        Assert.Equal("deviceId", ex.ParamName);
        _mockCardRepo.Verify(r => r.AddAsync(It.IsAny<DeviceCard>()), Times.Never);
        _mockDeviceRepo.Verify(r => r.UpdateAsync(It.IsAny<DeviceModel>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DeviceAlreadyHasCard_ThrowsInvalidOperationException()
    {
        // Arrange
        var deviceId = 1;
        var createDto = new CreateDeviceCardDto { SerialNumber = "NEW_SN" };
        var device = new DeviceModel { Id = deviceId, DeviceCardId = 10 }; // Device exists AND already has a card
        _mockDeviceRepo.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync(device);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(deviceId, createDto));
        _mockCardRepo.Verify(r => r.AddAsync(It.IsAny<DeviceCard>()), Times.Never);
        _mockDeviceRepo.Verify(r => r.UpdateAsync(It.IsAny<DeviceModel>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ValidDto_ReturnsTrue()
    {
        // Arrange
        var cardId = 1;
        var updateDto = new UpdateDeviceCardDto { SerialNumber = "UPDATED_SN" };
        var existingCard = new DeviceCard { Id = cardId, SerialNumber = "OLD_SN" };
        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId)).Returns(Task.FromResult<DeviceCard?>(existingCard));
        _mockCardRepo.Setup(r => r.UpdateAsync(It.IsAny<DeviceCard>())).Returns(Task.FromResult(existingCard));

        // Act
        var result = await _service.UpdateAsync(cardId, updateDto);

        // Assert
        Assert.True(result);
        _mockCardRepo.Verify(r => r.GetByIdAsync(cardId), Times.Once);
        _mockCardRepo.Verify(r => r.UpdateAsync(It.Is<DeviceCard>(c => c.Id == cardId && c.SerialNumber == updateDto.SerialNumber)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var cardId = 99;
        var updateDto = new UpdateDeviceCardDto { SerialNumber = "UPDATED_SN" };
        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId)).Returns(Task.FromResult<DeviceCard?>(null));

        // Act
        var result = await _service.UpdateAsync(cardId, updateDto);

        // Assert
        Assert.False(result);
        _mockCardRepo.Verify(r => r.GetByIdAsync(cardId), Times.Once);
        _mockCardRepo.Verify(r => r.UpdateAsync(It.IsAny<DeviceCard>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingCard_UnlinksDeviceAndDeletes_ReturnsTrue()
    {
        // Arrange
        var cardId = 1;
        var deviceId = 10;
        var existingCard = new DeviceCard { Id = cardId }; 
        var device = new DeviceModel { Id = deviceId, DeviceCardId = cardId }; // Device linked to the card

        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId)).Returns(Task.FromResult<DeviceCard?>(existingCard));
        _mockDeviceRepo.Setup(r => r.GetByDeviceCardIdAsync(cardId)).Returns(Task.FromResult<DeviceModel?>(device));
        _mockDeviceRepo.Setup(r => r.UpdateAsync(It.IsAny<DeviceModel>())).Returns(Task.FromResult(device)); 
        _mockCardRepo.Setup(r => r.DeleteAsync(cardId)).Returns(Task.FromResult(true));

        // Act
        var result = await _service.DeleteAsync(cardId);

        // Assert
        Assert.True(result);
        _mockDeviceRepo.Verify(r => r.GetByDeviceCardIdAsync(cardId), Times.Once);
        _mockDeviceRepo.Verify(r => r.UpdateAsync(It.Is<DeviceModel>(d => d.Id == deviceId && d.DeviceCardId == null)), Times.Once);
        _mockCardRepo.Verify(r => r.DeleteAsync(cardId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingCard_NoLinkedDevice_Deletes_ReturnsTrue()
    {
        // Arrange
        var cardId = 1;
        var existingCard = new DeviceCard { Id = cardId }; 

        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId)).Returns(Task.FromResult<DeviceCard?>(existingCard));
        _mockDeviceRepo.Setup(r => r.GetByDeviceCardIdAsync(cardId)).Returns(Task.FromResult<DeviceModel?>(null)); // No device found linked
        _mockCardRepo.Setup(r => r.DeleteAsync(cardId)).Returns(Task.FromResult(true));

        // Act
        var result = await _service.DeleteAsync(cardId);

        // Assert
        Assert.True(result);
        _mockDeviceRepo.Verify(r => r.GetByDeviceCardIdAsync(cardId), Times.Once);
        _mockDeviceRepo.Verify(r => r.UpdateAsync(It.IsAny<DeviceModel>()), Times.Never);
        _mockCardRepo.Verify(r => r.DeleteAsync(cardId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var cardId = 99;
        _mockCardRepo.Setup(r => r.GetByIdAsync(cardId)).Returns(Task.FromResult<DeviceCard?>(null));

        // Act
        var result = await _service.DeleteAsync(cardId);

        // Assert
        Assert.False(result);
        _mockCardRepo.Verify(r => r.GetByIdAsync(cardId), Times.Once);
        _mockDeviceRepo.Verify(r => r.GetByDeviceCardIdAsync(It.IsAny<int>()), Times.Never);
        _mockCardRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
} 