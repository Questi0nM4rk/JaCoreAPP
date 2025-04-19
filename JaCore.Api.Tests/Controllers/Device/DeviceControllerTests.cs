using Xunit;
using Moq;
using JaCore.Api.Controllers.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Dtos.Common;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;

namespace JaCore.Api.Tests.Controllers.Device;

public class DeviceControllerTests
{
    private readonly Mock<IDeviceService> _mockService;
    private readonly Mock<ILogger<DeviceController>> _mockLogger;
    private readonly DeviceController _controller;

    public DeviceControllerTests()
    {
        _mockService = new Mock<IDeviceService>();
        _mockLogger = new Mock<ILogger<DeviceController>>();
        _controller = new DeviceController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkObjectResult_WithPaginatedListDto()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var items = new List<DeviceDto> { new DeviceDto { Id = 1, Name = "Test Device", DeviceCardId = 10 } }; 
        var paginatedDto = new PaginatedListDto<DeviceDto>(items, 1, pageNumber, pageSize);
        _mockService.Setup(s => s.GetAllAsync(pageNumber, pageSize)).ReturnsAsync(paginatedDto);

        // Act
        var result = await _controller.GetAll(pageNumber, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsAssignableFrom<PaginatedListDto<DeviceDto>>(okResult.Value);
        Assert.Single(returnedDto.Items);
        Assert.Equal("Test Device", returnedDto.Items.First().Name); 
        Assert.Equal(10, returnedDto.Items.First().DeviceCardId);
        _mockService.Verify(s => s.GetAllAsync(pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkObjectResult_WithDeviceDto()
    {
        // Arrange
        var deviceId = 1;
        var deviceDto = new DeviceDto { Id = deviceId, Name = "Test Device", DeviceCardId = 10 }; 
        _mockService.Setup(s => s.GetByIdAsync(deviceId)).ReturnsAsync(deviceDto);

        // Act
        var result = await _controller.GetById(deviceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsAssignableFrom<DeviceDto>(okResult.Value);
        Assert.Equal(deviceId, returnedDto.Id);
        Assert.Equal("Test Device", returnedDto.Name);
        Assert.Equal(10, returnedDto.DeviceCardId);
        _mockService.Verify(s => s.GetByIdAsync(deviceId), Times.Once);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFoundResult()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((DeviceDto?)null);

        // Act
        var result = await _controller.GetById(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createDto = new CreateDeviceDto { Name = "New Device", CategoryId = 1, Description = "Desc" };
        var returnedDto = new DeviceDto { Id = 5, Name = "New Device", CategoryId = 1, Description = "Desc", DeviceCardId = null };
        _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(returnedDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(DeviceController.GetById), createdAtActionResult.ActionName);
        Assert.Equal(returnedDto.Id, createdAtActionResult.RouteValues!["id"]);
        var value = Assert.IsAssignableFrom<DeviceDto>(createdAtActionResult.Value);
        Assert.Equal(returnedDto.Id, value.Id);
        Assert.Equal(returnedDto.Name, value.Name); 
        Assert.Equal(returnedDto.DeviceCardId, value.DeviceCardId);
        _mockService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Update_ExistingId_ValidDto_ReturnsNoContentResult()
    {
        // Arrange
        var deviceId = 1;
        var updateDto = new UpdateDeviceDto { Name = "Updated Device", Description = "Desc" };
        _mockService.Setup(s => s.UpdateAsync(deviceId, updateDto)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(deviceId, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.UpdateAsync(deviceId, It.Is<UpdateDeviceDto>(dto => dto.Name == "Updated Device" && dto.Description == "Desc")), Times.Once);
    }

    [Fact]
    public async Task Update_NonExistingId_ServiceReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateDeviceDto { Name = "Update" };
        _mockService.Setup(s => s.UpdateAsync(99, updateDto)).ReturnsAsync(false);

        // Act
        var result = await _controller.Update(99, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ExistingId_ServiceReturnsTrue_ReturnsNoContent()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NonExistingId_ServiceReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
} 