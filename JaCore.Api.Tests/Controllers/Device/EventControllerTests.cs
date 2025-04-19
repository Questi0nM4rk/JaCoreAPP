using Xunit;
using Moq;
using JaCore.Api.Controllers.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Dtos.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using JaCore.Common.Device;

namespace JaCore.Api.Tests.Controllers.Device;

public class EventControllerTests
{
    private readonly Mock<IEventService> _mockService;
    private readonly Mock<ILogger<EventController>> _mockLogger;
    private readonly EventController _controller;

    public EventControllerTests()
    {
        _mockService = new Mock<IEventService>();
        _mockLogger = new Mock<ILogger<EventController>>();
        _controller = new EventController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByDeviceCardId_CardExists_ReturnsOkObjectResult_WithPaginatedListDto()
    {
        // Arrange
        var deviceCardId = 1;
        var pageNumber = 1;
        var pageSize = 10;
        var items = new List<EventDto> { new EventDto { Id = 1, Description = "Test Event", Type = EventType.Operation, DeviceCardId = deviceCardId, From = DateTimeOffset.UtcNow } };
        var paginatedDto = new PaginatedListDto<EventDto>(items, 1, pageNumber, pageSize);
        _mockService.Setup(s => s.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize)).ReturnsAsync(paginatedDto);

        // Act
        var result = await _controller.GetByDeviceCardId(deviceCardId, pageNumber, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsAssignableFrom<PaginatedListDto<EventDto>>(okResult.Value);
        Assert.Single(returnedDto.Items);
        Assert.Equal("Test Event", returnedDto.Items.First().Description);
        Assert.Equal(EventType.Operation, returnedDto.Items.First().Type);
        Assert.NotNull(returnedDto.Items.First().From);
        _mockService.Verify(s => s.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetByDeviceCardId_CardNotFoundOrNoEvents_ReturnsOkWithEmptyList()
    {
        // Arrange
        var deviceCardId = 99;
        var pageNumber = 1;
        var pageSize = 10;
        var emptyPaginatedDto = new PaginatedListDto<EventDto>(new List<EventDto>(), 0, pageNumber, pageSize);
        _mockService.Setup(s => s.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize)).ReturnsAsync(emptyPaginatedDto);

        // Act
        var result = await _controller.GetByDeviceCardId(deviceCardId, pageNumber, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<PaginatedListDto<EventDto>>(okResult.Value);
        Assert.Empty(returnedDto.Items);
        Assert.Equal(0, returnedDto.TotalCount);
        _mockService.Verify(s => s.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize), Times.Once);
    }
    
    [Fact]
    public async Task GetByDeviceCardId_InvalidCardId_ReturnsBadRequest()
    {
        // Arrange
        var deviceCardId = 0;
        var pageNumber = 1;
        var pageSize = 10;

        // Act
        var result = await _controller.GetByDeviceCardId(deviceCardId, pageNumber, pageSize);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockService.Verify(s => s.GetByDeviceCardIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkObjectResult_WithEventDto()
    {
        // Arrange
        var eventId = 1;
        var eventDto = new EventDto { Id = eventId, Description = "Test Event", Type = EventType.Service, DeviceCardId = 5, Who = "User1" };
        _mockService.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync(eventDto);

        // Act
        var result = await _controller.GetById(eventId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsAssignableFrom<EventDto>(okResult.Value);
        Assert.Equal(eventId, returnedDto.Id);
        Assert.Equal("Test Event", returnedDto.Description);
        Assert.Equal(EventType.Service, returnedDto.Type);
        Assert.Equal("User1", returnedDto.Who);
        _mockService.Verify(s => s.GetByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetById_EventNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var eventId = 99;
        _mockService.Setup(s => s.GetByIdAsync(eventId)).ReturnsAsync((EventDto?)null);

        // Act
        var result = await _controller.GetById(eventId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockService.Verify(s => s.GetByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createDto = new CreateEventDto { DeviceCardId = 1, Description = "New Event", Type = EventType.Calibration, Who = "Creator" };
        var returnedDto = new EventDto { Id = 10, DeviceCardId = 1, Description = "New Event", Type = EventType.Calibration, Who = "Creator", From = DateTimeOffset.UtcNow };
        _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(returnedDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result); 
        Assert.Equal(nameof(EventController.GetById), createdAtActionResult.ActionName);
        Assert.Equal(returnedDto.Id, createdAtActionResult.RouteValues!["id"]);
        Assert.Equal(returnedDto, createdAtActionResult.Value);
        _mockService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_CardNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var createDto = new CreateEventDto { DeviceCardId = 99, Description = "Test Event", Type = EventType.Maintenance, Who = "User" };
        var errorMessage = "Device card not found.";
        var paramName = nameof(createDto.DeviceCardId);
        _mockService.Setup(s => s.CreateAsync(createDto))
                    .ThrowsAsync(new ArgumentException(errorMessage, paramName));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
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
        var createDto = new CreateEventDto { DeviceCardId = 1, Description = "Test Event", Type = EventType.Malfunction, Who = "User" };
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
    public async Task Update_ExistingId_ValidDto_ReturnsNoContentResult()
    {
        // Arrange
        var eventId = 1;
        var updateDto = new UpdateEventDto { Description = "Updated Event", Type = EventType.Service, To = DateTimeOffset.UtcNow };
        _mockService.Setup(s => s.UpdateAsync(eventId, updateDto)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(eventId, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.UpdateAsync(eventId, It.Is<UpdateEventDto>(dto => 
            dto.Description == "Updated Event" && 
            dto.Type == EventType.Service &&
            dto.To != null
            )), Times.Once);
    }

    [Fact]
    public async Task Update_EventNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var eventId = 99;
        var updateDto = new UpdateEventDto { Description = "Updated Event", Type = EventType.Operation, To = DateTimeOffset.UtcNow };
        _mockService.Setup(s => s.UpdateAsync(eventId, updateDto)).ReturnsAsync(false);

        // Act
        var result = await _controller.Update(eventId, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.UpdateAsync(eventId, updateDto), Times.Once);
    }

    [Fact]
    public async Task Update_ServiceThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var eventId = 1;
        var updateDto = new UpdateEventDto { Description = "Updated Event", Type = EventType.Maintenance, To = DateTimeOffset.UtcNow };
        _mockService.Setup(s => s.UpdateAsync(eventId, updateDto)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Update(eventId, updateDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
        _mockService.Verify(s => s.UpdateAsync(eventId, updateDto), Times.Once);
    }

    [Fact]
    public async Task Delete_EventExists_ReturnsNoContentResult()
    {
        // Arrange
        var eventId = 1;
        _mockService.Setup(s => s.DeleteAsync(eventId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(eventId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task Delete_EventNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var eventId = 99;
        _mockService.Setup(s => s.DeleteAsync(eventId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(eventId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.DeleteAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task Delete_ServiceThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var eventId = 1;
        _mockService.Setup(s => s.DeleteAsync(eventId)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Delete(eventId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
        _mockService.Verify(s => s.DeleteAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetByDeviceCardId_CardExists_ReturnsOkObjectResult_WithTestEvent()
    {
        // Arrange
        var cardId = 1;
        var testEvent = new EventDto { Id = 1, DeviceCardId = cardId, Description = "Test Event Message", Type = EventType.Operation };
        var events = new List<EventDto> { testEvent };
        var paginatedList = new PaginatedListDto<EventDto>(events, 1, 1, 10);
        _mockService.Setup(s => s.GetByDeviceCardIdAsync(cardId, 1, 10)).ReturnsAsync(paginatedList);

        // Act
        var result = await _controller.GetByDeviceCardId(cardId, 1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<PaginatedListDto<EventDto>>(okResult.Value);
        Assert.Single(returnedList.Items);
        Assert.Equal("Test Event Message", returnedList.Items.First().Description);
        _mockService.Verify(s => s.GetByDeviceCardIdAsync(cardId, 1, 10), Times.Once);
    }
} 