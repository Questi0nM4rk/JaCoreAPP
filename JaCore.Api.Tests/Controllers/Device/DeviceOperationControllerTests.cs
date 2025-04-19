using Xunit;
using Moq;
using JaCore.Api.Controllers.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Dtos.Device;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JaCore.Api.Tests.Controllers.Device;

public class DeviceOperationControllerTests
{
    private readonly Mock<IDeviceOperationService> _mockService;
    private readonly Mock<ILogger<DeviceOperationController>> _mockLogger;
    private readonly DeviceOperationController _controller;

    public DeviceOperationControllerTests()
    {
        _mockService = new Mock<IDeviceOperationService>();
        _mockLogger = new Mock<ILogger<DeviceOperationController>>();
        _controller = new DeviceOperationController(_mockService.Object, _mockLogger.Object);
        // Mock HttpContext and User for authorization checks if needed
        // _controller.ControllerContext = new ControllerContext {
        //     HttpContext = new DefaultHttpContext { User = ... }
        // };
    }

    [Fact]
    public async Task GetByDeviceCardId_CardExists_ReturnsOkObjectResult()
    {
        // Arrange
        var deviceCardId = 1;
        var pageNumber = 1;
        var pageSize = 10;
        var items = new List<DeviceOperationDto> { new DeviceOperationDto { Id = 1, Name = "Op1", DeviceCardId = deviceCardId } };
        var paginatedDto = new PaginatedListDto<DeviceOperationDto>(items, 1, pageNumber, pageSize);
        _mockService.Setup(s => s.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize)).ReturnsAsync(paginatedDto);

        // Act
        var result = await _controller.GetByDeviceCardId(deviceCardId, pageNumber, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<PaginatedListDto<DeviceOperationDto>>(okResult.Value);
        Assert.Single(returnedDto.Items);
        Assert.Equal(deviceCardId, returnedDto.Items.First().DeviceCardId);
        _mockService.Verify(s => s.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetByDeviceCardId_CardNotFoundOrNoOps_ReturnsOkWithEmptyList()
    {
        // Arrange
        var deviceCardId = 99;
        var pageNumber = 1;
        var pageSize = 10;
        var emptyPaginatedDto = new PaginatedListDto<DeviceOperationDto>(new List<DeviceOperationDto>(), 0, pageNumber, pageSize);
        _mockService.Setup(s => s.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize)).ReturnsAsync(emptyPaginatedDto);

        // Act
        var result = await _controller.GetByDeviceCardId(deviceCardId, pageNumber, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<PaginatedListDto<DeviceOperationDto>>(okResult.Value);
        Assert.Empty(returnedDto.Items);
        Assert.Equal(0, returnedDto.TotalCount);
        _mockService.Verify(s => s.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetById_OperationExists_ReturnsOkObjectResult()
    {
        // Arrange
        var opId = 1;
        var opDto = new DeviceOperationDto { Id = opId, Name = "Op1", DeviceCardId = 5 };
        _mockService.Setup(s => s.GetByIdAsync(opId)).ReturnsAsync(opDto);

        // Act
        var result = await _controller.GetById(opId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<DeviceOperationDto>(okResult.Value);
        Assert.Equal(opId, returnedDto.Id);
        _mockService.Verify(s => s.GetByIdAsync(opId), Times.Once);
    }

    [Fact]
    public async Task GetById_OperationNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var opId = 99;
        _mockService.Setup(s => s.GetByIdAsync(opId)).ReturnsAsync((DeviceOperationDto?)null);

        // Act
        var result = await _controller.GetById(opId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockService.Verify(s => s.GetByIdAsync(opId), Times.Once);
    }

    [Fact]
    public async Task Create_ValidData_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createDto = new CreateDeviceOperationDto { Name = "New Op", DeviceCardId = 1 };
        var createdDto = new DeviceOperationDto { Id = 10, Name = createDto.Name, DeviceCardId = createDto.DeviceCardId };
        _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_controller.GetById), createdAtActionResult.ActionName);
        Assert.Equal(createdDto.Id, createdAtActionResult.RouteValues!["id"]);
        Assert.Equal(createdDto, createdAtActionResult.Value);
        _mockService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_CardNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var createDto = new CreateDeviceOperationDto { Name = "New Op", DeviceCardId = 99 };
        var errorMessage = "Device card not found.";
        var paramName = nameof(createDto.DeviceCardId);
        _mockService.Setup(s => s.CreateAsync(createDto))
                    .ThrowsAsync(new ArgumentException(errorMessage, paramName));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        // Check ModelState for the specific error
        var modelState = Assert.IsAssignableFrom<SerializableError>(badRequestResult.Value);
        Assert.True(modelState.ContainsKey(paramName));
        var errors = modelState[paramName] as string[];
        Assert.NotNull(errors);
        Assert.Contains("Device card not found. (Parameter 'DeviceCardId')", errors);
        _mockService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }
    
    [Fact]
    public async Task Create_ServiceThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var createDto = new CreateDeviceOperationDto { Name = "New Op", DeviceCardId = 1 };
        _mockService.Setup(s => s.CreateAsync(createDto)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
        _mockService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Update_OperationExists_ReturnsNoContentResult()
    {
        // Arrange
        var opId = 1;
        var updateDto = new UpdateDeviceOperationDto { Name = "Updated Op" };
        _mockService.Setup(s => s.UpdateAsync(opId, updateDto)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(opId, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.UpdateAsync(opId, updateDto), Times.Once);
    }

    [Fact]
    public async Task Update_OperationNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var opId = 99;
        var updateDto = new UpdateDeviceOperationDto { Name = "Updated Op" };
        _mockService.Setup(s => s.UpdateAsync(opId, updateDto)).ReturnsAsync(false);

        // Act
        var result = await _controller.Update(opId, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.UpdateAsync(opId, updateDto), Times.Once);
    }
    
    [Fact]
    public async Task Update_ServiceThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var opId = 1;
        var updateDto = new UpdateDeviceOperationDto { Name = "Updated Op" };
        _mockService.Setup(s => s.UpdateAsync(opId, updateDto)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Update(opId, updateDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
        _mockService.Verify(s => s.UpdateAsync(opId, updateDto), Times.Once);
    }

    [Fact]
    public async Task Delete_OperationExists_ReturnsNoContentResult()
    {
        // Arrange
        var opId = 1;
        _mockService.Setup(s => s.DeleteAsync(opId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(opId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAsync(opId), Times.Once);
    }

    [Fact]
    public async Task Delete_OperationNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var opId = 99;
        _mockService.Setup(s => s.DeleteAsync(opId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(opId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.DeleteAsync(opId), Times.Once);
    }
    
    [Fact]
    public async Task Delete_ServiceThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var opId = 1;
        _mockService.Setup(s => s.DeleteAsync(opId)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Delete(opId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
        _mockService.Verify(s => s.DeleteAsync(opId), Times.Once);
    }
} 