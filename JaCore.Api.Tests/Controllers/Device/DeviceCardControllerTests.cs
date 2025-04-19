using Xunit;
using Moq;
using JaCore.Api.Controllers.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Dtos.Device;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace JaCore.Api.Tests.Controllers.Device;

public class DeviceCardControllerTests
{
    private readonly Mock<IDeviceCardService> _mockService;
    private readonly Mock<ILogger<DeviceCardController>> _mockLogger;
    private readonly DeviceCardController _controller;

    public DeviceCardControllerTests()
    {
        _mockService = new Mock<IDeviceCardService>();
        _mockLogger = new Mock<ILogger<DeviceCardController>>();
        _controller = new DeviceCardController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByDeviceId_CardExists_ReturnsOkObjectResult()
    {
        // Arrange
        var deviceId = 1;
        var cardDto = new DeviceCardDto { Id = 10, SerialNumber = "SN001" };
        _mockService.Setup(s => s.GetByDeviceIdAsync(deviceId)).ReturnsAsync(cardDto);

        // Act
        var result = await _controller.GetByDeviceId(deviceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(cardDto, okResult.Value);
        _mockService.Verify(s => s.GetByDeviceIdAsync(deviceId), Times.Once);
    }

    [Fact]
    public async Task GetByDeviceId_CardNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var deviceId = 99;
        _mockService.Setup(s => s.GetByDeviceIdAsync(deviceId)).ReturnsAsync((DeviceCardDto?)null);

        // Act
        var result = await _controller.GetByDeviceId(deviceId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockService.Verify(s => s.GetByDeviceIdAsync(deviceId), Times.Once);
    }
    
    [Fact]
    public async Task GetById_CardExists_ReturnsOkObjectResult()
    {
        // Arrange
        var cardId = 10;
        var cardDto = new DeviceCardDto { Id = cardId, SerialNumber = "SN001" };
        _mockService.Setup(s => s.GetByIdAsync(cardId)).ReturnsAsync(cardDto);

        // Act
        var result = await _controller.GetById(cardId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(cardDto, okResult.Value);
         _mockService.Verify(s => s.GetByIdAsync(cardId), Times.Once);
    }

    [Fact]
    public async Task GetById_CardNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var cardId = 99;
        _mockService.Setup(s => s.GetByIdAsync(cardId)).ReturnsAsync((DeviceCardDto?)null);

        // Act
        var result = await _controller.GetById(cardId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
         _mockService.Verify(s => s.GetByIdAsync(cardId), Times.Once);
    }

    [Fact]
    public async Task Create_ValidData_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var deviceId = 1;
        var createDto = new CreateDeviceCardDto { SerialNumber = "NEW_SN" };
        var createdDto = new DeviceCardDto { Id = 15, SerialNumber = "NEW_SN" };
        _mockService.Setup(s => s.CreateAsync(deviceId, createDto)).ReturnsAsync(createdDto);

        // Act
        var result = await _controller.Create(deviceId, createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(DeviceCardController.GetById), createdAtResult.ActionName);
        Assert.Equal(createdDto.Id, createdAtResult.RouteValues!["id"]);
        Assert.Equal(createdDto, createdAtResult.Value);
        _mockService.Verify(s => s.CreateAsync(deviceId, createDto), Times.Once);
    }
    
    [Fact]
    public async Task Create_DeviceNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var deviceId = 99;
        var createDto = new CreateDeviceCardDto { SerialNumber = "NEW_SN" };
        _mockService.Setup(s => s.CreateAsync(deviceId, createDto))
                    .ThrowsAsync(new ArgumentException("Device not found", "deviceId"));

        // Act
        var result = await _controller.Create(deviceId, createDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal("Not Found", problemDetails.Title);
        _mockService.Verify(s => s.CreateAsync(deviceId, createDto), Times.Once);
    }
    
    [Fact]
    public async Task Create_CardAlreadyExists_ReturnsConflictResult()
    {
        // Arrange
        var deviceId = 1;
        var createDto = new CreateDeviceCardDto { SerialNumber = "NEW_SN" };
        _mockService.Setup(s => s.CreateAsync(deviceId, createDto))
                    .ThrowsAsync(new InvalidOperationException("Card already exists"));

        // Act
        var result = await _controller.Create(deviceId, createDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(conflictResult.Value);
        Assert.Equal("Conflict", problemDetails.Title);
        _mockService.Verify(s => s.CreateAsync(deviceId, createDto), Times.Once);
    }

    [Fact]
    public async Task Update_CardExists_ReturnsNoContentResult()
    {
        // Arrange
        var cardId = 10;
        var updateDto = new UpdateDeviceCardDto { SerialNumber = "UPDATED_SN" };
        _mockService.Setup(s => s.UpdateAsync(cardId, updateDto)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(cardId, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.UpdateAsync(cardId, updateDto), Times.Once);
    }

    [Fact]
    public async Task Update_CardNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var cardId = 99;
        var updateDto = new UpdateDeviceCardDto { SerialNumber = "UPDATED_SN" };
        _mockService.Setup(s => s.UpdateAsync(cardId, updateDto)).ReturnsAsync(false);

        // Act
        var result = await _controller.Update(cardId, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.UpdateAsync(cardId, updateDto), Times.Once);
    }

    [Fact]
    public async Task Delete_CardExists_ReturnsNoContentResult()
    {
        // Arrange
        var cardId = 10;
        _mockService.Setup(s => s.DeleteAsync(cardId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(cardId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAsync(cardId), Times.Once);
    }

    [Fact]
    public async Task Delete_CardNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var cardId = 99;
        _mockService.Setup(s => s.DeleteAsync(cardId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(cardId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.DeleteAsync(cardId), Times.Once);
    }
} 