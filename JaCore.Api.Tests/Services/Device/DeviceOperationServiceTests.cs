using Xunit;
using Moq;
using JaCore.Api.Services.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Models.Device;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JaCore.Api.Interfaces.Services;

namespace JaCore.Api.Tests.Services.Device;

public class DeviceOperationServiceTests
{
    private readonly Mock<IDeviceOperationRepository> _mockOpRepo;
    private readonly Mock<IDeviceCardRepository> _mockCardRepo;
    private readonly Mock<ILogger<DeviceOperationService>> _mockLogger;
    private readonly DeviceOperationService _service;

    public DeviceOperationServiceTests()
    {
        _mockOpRepo = new Mock<IDeviceOperationRepository>();
        _mockCardRepo = new Mock<IDeviceCardRepository>();
        _mockLogger = new Mock<ILogger<DeviceOperationService>>();
        _service = new DeviceOperationService(_mockOpRepo.Object, _mockCardRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByDeviceCardIdAsync_CardExists_ReturnsPaginatedDto()
    {
        // Arrange
        var deviceCardId = 1;
        var pageNumber = 1;
        var pageSize = 10;
        var operations = new List<DeviceOperation> { new DeviceOperation { Id = 1, Name = "Op1", DeviceCardId = deviceCardId } };
        // Revert: Repository returns PaginatedListDto<DeviceOperation>
        var paginatedDtoFromRepo = new PaginatedListDto<DeviceOperation>(operations, 1, pageNumber, pageSize);

        _mockCardRepo.Setup(r => r.ExistsAsync(deviceCardId)).Returns(Task.FromResult(true));
        // Revert mock setup to return Task<PaginatedListDto<DeviceOperation>>
        _mockOpRepo.Setup(r => r.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize)).Returns(Task.FromResult(paginatedDtoFromRepo));

        // Act
        var result = await _service.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize);

        // Assert (Service returns PaginatedListDto<DeviceOperationDto>)
        Assert.NotNull(result);
        Assert.Single(result.Items);
        // Restore pagination asserts
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal("Op1", result.Items.First().Name);
        Assert.Equal(deviceCardId, result.Items.First().DeviceCardId);
        _mockCardRepo.Verify(r => r.ExistsAsync(deviceCardId), Times.Once);
        _mockOpRepo.Verify(r => r.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetByDeviceCardIdAsync_CardNotFound_ReturnsEmptyList()
    {
        // Arrange
        var deviceCardId = 99;
        var pageNumber = 1;
        var pageSize = 10;
        _mockCardRepo.Setup(r => r.ExistsAsync(deviceCardId)).ReturnsAsync(false);

        // Act
        var result = await _service.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        _mockCardRepo.Verify(r => r.ExistsAsync(deviceCardId), Times.Once);
        _mockOpRepo.Verify(r => r.GetByDeviceCardIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_OperationExists_ReturnsDto()
    {
        // Arrange
        var opId = 1;
        var operation = new DeviceOperation { Id = opId, Name = "Op1", DeviceCardId = 5 };
        _mockOpRepo.Setup(r => r.GetByIdAsync(opId)).ReturnsAsync(operation);

        // Act
        var result = await _service.GetByIdAsync(opId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(opId, result.Id);
        Assert.Equal(operation.Name, result.Name);
        Assert.Equal(operation.DeviceCardId, result.DeviceCardId);
        _mockOpRepo.Verify(r => r.GetByIdAsync(opId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_OperationNotFound_ReturnsNull()
    {
        // Arrange
        var opId = 99;
        _mockOpRepo.Setup(r => r.GetByIdAsync(opId)).ReturnsAsync((DeviceOperation?)null);

        // Act
        var result = await _service.GetByIdAsync(opId);

        // Assert
        Assert.Null(result);
        _mockOpRepo.Verify(r => r.GetByIdAsync(opId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CardExists_ValidDto_ReturnsCreatedDto()
    {
        // Arrange
        var deviceCardId = 1;
        var createDto = new CreateDeviceOperationDto { DeviceCardId = deviceCardId, Name = "New Op" };
        var addedOperation = new DeviceOperation { Id = 10 }; // Operation after AddAsync

        _mockCardRepo.Setup(r => r.ExistsAsync(deviceCardId)).ReturnsAsync(true);
        _mockOpRepo.Setup(r => r.AddAsync(It.IsAny<DeviceOperation>()))
                   .Callback<DeviceOperation>(op => { 
                       op.Id = addedOperation.Id;
                       op.Name = createDto.Name;
                       op.DeviceCardId = createDto.DeviceCardId;
                       addedOperation = op;
                   })
                   .ReturnsAsync(() => addedOperation);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedOperation.Id, result.Id);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.DeviceCardId, result.DeviceCardId);
        _mockCardRepo.Verify(r => r.ExistsAsync(deviceCardId), Times.Once);
        _mockOpRepo.Verify(r => r.AddAsync(It.Is<DeviceOperation>(op => op.Name == createDto.Name && op.DeviceCardId == deviceCardId)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CardNotFound_ThrowsArgumentException()
    {
        // Arrange
        var deviceCardId = 99;
        var createDto = new CreateDeviceOperationDto { DeviceCardId = deviceCardId, Name = "New Op" };
        _mockCardRepo.Setup(r => r.ExistsAsync(deviceCardId)).ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
        Assert.Equal(nameof(createDto.DeviceCardId), ex.ParamName);
        _mockOpRepo.Verify(r => r.AddAsync(It.IsAny<DeviceOperation>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Too long name.....................................................................................................")] // > 100 chars
    public async Task CreateAsync_InvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Arrange
        var deviceCardId = 1;
        var createDto = new CreateDeviceOperationDto { DeviceCardId = deviceCardId, Name = invalidName ?? string.Empty };
        _mockCardRepo.Setup(r => r.ExistsAsync(deviceCardId)).Returns(Task.FromResult(true)); // Assume card exists

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
        Assert.Equal(nameof(createDto.Name), ex.ParamName);
        _mockOpRepo.Verify(r => r.AddAsync(It.IsAny<DeviceOperation>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OperationExists_ValidDto_ReturnsTrue()
    {
        // Arrange
        var opId = 1;
        var updateDto = new UpdateDeviceOperationDto { Name = "Updated Op" };
        var existingOp = new DeviceOperation { Id = opId, Name = "Old Op", DeviceCardId = 5 };
        _mockOpRepo.Setup(r => r.GetByIdAsync(opId)).Returns(Task.FromResult<DeviceOperation?>(existingOp));
        _mockOpRepo.Setup(r => r.UpdateAsync(It.IsAny<DeviceOperation>())).Returns(Task.FromResult(existingOp));

        // Act
        var result = await _service.UpdateAsync(opId, updateDto);

        // Assert
        Assert.True(result);
        _mockOpRepo.Verify(r => r.GetByIdAsync(opId), Times.Once);
        _mockOpRepo.Verify(r => r.UpdateAsync(It.Is<DeviceOperation>(op => op.Id == opId && op.Name == updateDto.Name)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_OperationNotFound_ReturnsFalse()
    {
        // Arrange
        var opId = 99;
        var updateDto = new UpdateDeviceOperationDto { Name = "Update" };
        _mockOpRepo.Setup(r => r.GetByIdAsync(opId)).Returns(Task.FromResult<DeviceOperation?>(null));

        // Act
        var result = await _service.UpdateAsync(opId, updateDto);

        // Assert
        Assert.False(result);
        _mockOpRepo.Verify(r => r.GetByIdAsync(opId), Times.Once);
        _mockOpRepo.Verify(r => r.UpdateAsync(It.IsAny<DeviceOperation>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_OperationExists_ReturnsTrue()
    {
        // Arrange
        var opId = 1;
        var existingOp = new DeviceOperation { Id = opId };
        _mockOpRepo.Setup(r => r.GetByIdAsync(opId)).Returns(Task.FromResult<DeviceOperation?>(existingOp));
        _mockOpRepo.Setup(r => r.DeleteAsync(opId)).Returns(Task.FromResult(true));

        // Act
        var result = await _service.DeleteAsync(opId);

        // Assert
        Assert.True(result);
        _mockOpRepo.Verify(r => r.GetByIdAsync(opId), Times.Once);
        _mockOpRepo.Verify(r => r.DeleteAsync(opId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_OperationNotFound_ReturnsFalse()
    {
        // Arrange
        var opId = 99;
        _mockOpRepo.Setup(r => r.GetByIdAsync(opId)).Returns(Task.FromResult<DeviceOperation?>(null));

        // Act
        var result = await _service.DeleteAsync(opId);

        // Assert
        Assert.False(result);
        _mockOpRepo.Verify(r => r.GetByIdAsync(opId), Times.Once);
        _mockOpRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
} 