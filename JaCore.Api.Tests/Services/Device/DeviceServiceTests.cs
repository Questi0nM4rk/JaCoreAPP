using Xunit;
using Moq;
using JaCore.Api.Services.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.Device;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Dtos.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using DeviceModel = JaCore.Api.Models.Device.Device;
using System.Linq.Expressions;
using JaCore.Common.Device;

namespace JaCore.Api.Tests.Services.Device;

public class DeviceServiceTests
{
    private readonly Mock<IDeviceRepository> _mockRepo;
    private readonly Mock<ILogger<DeviceService>> _mockLogger;
    private readonly DeviceService _service;

    public DeviceServiceTests()
    {
        _mockRepo = new Mock<IDeviceRepository>();
        _mockLogger = new Mock<ILogger<DeviceService>>();
        _service = new DeviceService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedListDto()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var devices = new List<DeviceModel> { new DeviceModel { Id = 1, Name = "Test Device", DeviceCardId = 10 } };
        // Provide explicit null/It.IsAny for all parameters to avoid CS0854
        _mockRepo.Setup(r => r.GetAllAsync(pageNumber, pageSize, null, It.IsAny<bool>()))
                 .Returns(Task.FromResult<IEnumerable<DeviceModel>>(devices));

        // Act
        var result = await _service.GetAllAsync(pageNumber, pageSize);

        // Assert (Service maps IEnumerable<DeviceModel> to PaginatedListDto<DeviceDto>)
        Assert.NotNull(result);
        Assert.Single(result.Items);
        var firstItem = result.Items.First();
        Assert.Equal(1, firstItem.Id);
        Assert.Equal("Test Device", firstItem.Name);
        Assert.Equal(10, firstItem.DeviceCardId);
        // Provide explicit null/It.IsAny for all parameters in Verify
        _mockRepo.Verify(r => r.GetAllAsync(pageNumber, pageSize, null, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDeviceDto()
    {
        // Arrange
        var device = new DeviceModel { Id = 1, Name = "Test Device", DeviceCardId = 10 };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(device);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Device", result.Name);
        Assert.Equal(10, result.DeviceCardId);
        _mockRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((DeviceModel?)null);

        // Act
        var result = await _service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
        _mockRepo.Verify(r => r.GetByIdAsync(99), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedDto()
    {
        // Arrange
        var createDto = new CreateDeviceDto { Name = "New Device", CategoryId = 1, Description = "Desc" };
        var addedDevice = new DeviceModel();

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<DeviceModel>()))
                 .Callback<DeviceModel>(d => { 
                     d.Id = 5;
                     d.Name = createDto.Name;
                     d.CategoryId = createDto.CategoryId;
                     d.Description = createDto.Description;
                     addedDevice = d;
                 })
                 .ReturnsAsync(() => addedDevice);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("New Device", result.Name);
        _mockRepo.Verify(r => r.AddAsync(It.Is<DeviceModel>(d => 
            d.Name == "New Device" && 
            d.CategoryId == 1 &&
            d.Description == "Desc"
            )), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ValidDto_ReturnsTrue()
    {
        // Arrange
        var id = 1;
        var deviceCardId = 10;
        var updateDto = new UpdateDeviceDto
        {
            Name = "Updated Device Name",
            Description = "Updated Description",
            DataState = DeviceDataState.Modified,
            OperationalState = DeviceOperationalState.Active,
            CategoryId = 2,
            Properties = "{\"Key\":\"Value\"}",
            OrderIndex = 1,
            IsCompleted = true
        };

        var existingDevice = new DeviceModel // Use alias
        {
            Id = id,
            DeviceCardId = deviceCardId,
            Name = "Old Name",
            Description = "Old Desc",
            DataState = DeviceDataState.Idle,
            OperationalState = DeviceOperationalState.Idle,
            CategoryId = 1,
            Properties = "{\"OldKey\":\"OldValue\"}",
            OrderIndex = 0,
            IsCompleted = false
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(existingDevice);
        _mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<DeviceModel>()))
                 .ReturnsAsync(existingDevice);
        _mockRepo.Setup(repo => repo.ExistsAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateAsync(id, updateDto);

        // Assert
        Assert.True(result);
        _mockRepo.Verify(repo => repo.GetByIdAsync(id), Times.Once);
        _mockRepo.Verify(repo => repo.UpdateAsync(It.Is<DeviceModel>(d =>
            d.Id == id &&
            d.Name == updateDto.Name &&
            d.Description == updateDto.Description &&
            d.DataState == updateDto.DataState &&
            d.OperationalState == updateDto.OperationalState &&
            d.CategoryId == updateDto.CategoryId &&
            d.Properties == updateDto.Properties &&
            d.OrderIndex == updateDto.OrderIndex &&
            d.IsCompleted == updateDto.IsCompleted &&
            d.DeviceCardId == existingDevice.DeviceCardId
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var updateDto = new UpdateDeviceDto { Name = "Updated Device" };
        _mockRepo.Setup(r => r.GetByIdAsync(99)).Returns(Task.FromResult<DeviceModel?>(null));

        // Act
        var result = await _service.UpdateAsync(99, updateDto);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.GetByIdAsync(99), Times.Once);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<DeviceModel>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var existingDevice = new DeviceModel { Id = 1 };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).Returns(Task.FromResult<DeviceModel?>(existingDevice));
        _mockRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.FromResult(true)); 

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepo.Verify(r => r.DeleteAsync(1), Times.Once); 
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).Returns(Task.FromResult<DeviceModel?>(null));

        // Act
        var result = await _service.DeleteAsync(99);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.GetByIdAsync(99), Times.Once);
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never); 
    }
} 