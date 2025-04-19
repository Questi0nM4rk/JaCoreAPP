using Xunit;
using Moq;
using JaCore.Api.Services.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Models.Device;
using JaCore.Api.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JaCore.Common.Device;

namespace JaCore.Api.Tests.Services.Device;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _mockEventRepo;
    private readonly Mock<IDeviceCardRepository> _mockCardRepo;
    private readonly Mock<ILogger<EventService>> _mockLogger;
    private readonly EventService _service;

    public EventServiceTests()
    {
        _mockEventRepo = new Mock<IEventRepository>();
        _mockCardRepo = new Mock<IDeviceCardRepository>();
        _mockLogger = new Mock<ILogger<EventService>>();
        _service = new EventService(_mockEventRepo.Object, _mockCardRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByDeviceCardIdAsync_CardExists_ReturnsPaginatedDto()
    {
        // Arrange
        var deviceCardId = 1;
        var pageNumber = 1;
        var pageSize = 10;
        var events = new List<Event> { new Event { Id = 1, Type = EventType.Operation, Description = "Event1", DeviceCardId = deviceCardId, From = DateTimeOffset.UtcNow } };
        // Revert: Repository returns PaginatedListDto<Event>
        var paginatedDtoFromRepo = new PaginatedListDto<Event>(events, 1, pageNumber, pageSize);

        _mockCardRepo.Setup(r => r.ExistsAsync(deviceCardId)).Returns(Task.FromResult(true));
        // Revert mock setup to return Task<PaginatedListDto<Event>>
        _mockEventRepo.Setup(r => r.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize)).Returns(Task.FromResult(paginatedDtoFromRepo));

        // Act
        var result = await _service.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize);

        // Assert (Service returns PaginatedListDto<EventDto>)
        Assert.NotNull(result);
        Assert.Single(result.Items);
        // Restore pagination asserts if service maps them directly
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal("Event1", result.Items.First().Description);
        Assert.Equal(deviceCardId, result.Items.First().DeviceCardId);
        Assert.Equal(EventType.Operation, result.Items.First().Type);
        _mockCardRepo.Verify(r => r.ExistsAsync(deviceCardId), Times.Once);
        _mockEventRepo.Verify(r => r.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize), Times.Once);
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
        _mockEventRepo.Verify(r => r.GetByDeviceCardIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_EventExists_ReturnsDto()
    {
        // Arrange
        var eventId = 1;
        var ev = new Event { Id = eventId, Description = "Event1", DeviceCardId = 5, Type = EventType.Service, From = DateTimeOffset.UtcNow };
        _mockEventRepo.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(ev);

        // Act
        var result = await _service.GetByIdAsync(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventId, result.Id);
        Assert.Equal(ev.Description, result.Description);
        Assert.Equal(ev.DeviceCardId, result.DeviceCardId);
        Assert.Equal(ev.Type, result.Type);
        Assert.Equal(ev.From, result.From);
        _mockEventRepo.Verify(r => r.GetByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_EventNotFound_ReturnsNull()
    {
        // Arrange
        var eventId = 99;
        _mockEventRepo.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync((Event?)null);

        // Act
        var result = await _service.GetByIdAsync(eventId);

        // Assert
        Assert.Null(result);
        _mockEventRepo.Verify(r => r.GetByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CardExists_ValidDto_ReturnsCreatedDto()
    {
        // Arrange
        var deviceCardId = 1;
        var createDto = new CreateEventDto { DeviceCardId = deviceCardId, Type = EventType.Calibration, Description = "New Event", Who = "User1" };
        var addedEvent = new Event { Id = 10 }; // Event after AddAsync

        _mockCardRepo.Setup(r => r.ExistsAsync(deviceCardId)).ReturnsAsync(true);
        _mockEventRepo.Setup(r => r.AddAsync(It.IsAny<Event>()))
                      .Callback<Event>(ev => {
                          ev.Id = addedEvent.Id;
                          ev.Description = createDto.Description;
                          ev.Type = createDto.Type;
                          ev.DeviceCardId = createDto.DeviceCardId;
                          ev.Who = createDto.Who;
                          ev.From = DateTimeOffset.UtcNow; // Service sets timestamp
                          addedEvent = ev;
                      })
                      .ReturnsAsync(() => addedEvent);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedEvent.Id, result.Id);
        Assert.Equal(createDto.Description, result.Description);
        Assert.Equal(createDto.Type, result.Type);
        Assert.Equal(createDto.DeviceCardId, result.DeviceCardId);
        Assert.Equal(createDto.Who, result.Who);
        Assert.NotNull(result.From);
        _mockCardRepo.Verify(r => r.ExistsAsync(deviceCardId), Times.Once);
        _mockEventRepo.Verify(r => r.AddAsync(It.Is<Event>(ev => ev.Description == createDto.Description && ev.DeviceCardId == deviceCardId && ev.Type == createDto.Type && ev.Who == createDto.Who)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CardNotFound_ThrowsArgumentException()
    {
        // Arrange
        var deviceCardId = 99;
        var createDto = new CreateEventDto { DeviceCardId = deviceCardId, Type = EventType.Maintenance, Description = "Test" };
        _mockCardRepo.Setup(r => r.ExistsAsync(deviceCardId)).ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
        Assert.Equal(nameof(createDto.DeviceCardId), ex.ParamName);
        _mockEventRepo.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_InvalidDescription_ThrowsArgumentException(string? invalidDescription)
    {
        // Arrange
        var deviceCardId = 1;
        var createDto = new CreateEventDto { DeviceCardId = deviceCardId, Type = EventType.Operation, Description = invalidDescription };
        _mockCardRepo.Setup(r => r.ExistsAsync(deviceCardId)).Returns(Task.FromResult(true)); // Assume card exists

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
        Assert.Equal(nameof(createDto.Description), ex.ParamName);
        _mockEventRepo.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EventExists_ValidDto_ReturnsTrue()
    {
        // Arrange
        var eventId = 1;
        var updateDto = new UpdateEventDto { Type = EventType.Service, Description = "Updated Event", To = DateTimeOffset.UtcNow };
        var existingEvent = new Event { Id = eventId, Description = "Old Event", Type = EventType.Calibration, DeviceCardId = 5, From = DateTimeOffset.UtcNow.AddMinutes(-5) };
        _mockEventRepo.Setup(r => r.GetByIdAsync(eventId)).Returns(Task.FromResult<Event?>(existingEvent));
        _mockEventRepo.Setup(r => r.UpdateAsync(It.IsAny<Event>())).Returns(Task.FromResult(existingEvent));

        // Act
        var result = await _service.UpdateAsync(eventId, updateDto);

        // Assert
        Assert.True(result);
        _mockEventRepo.Verify(r => r.GetByIdAsync(eventId), Times.Once);
        _mockEventRepo.Verify(r => r.UpdateAsync(It.Is<Event>(ev =>
            ev.Id == eventId &&
            ev.Description == updateDto.Description &&
            ev.Type == updateDto.Type &&
            ev.To == updateDto.To &&
            ev.From == existingEvent.From &&
            ev.DeviceCardId == existingEvent.DeviceCardId &&
            ev.Who == existingEvent.Who
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_EventNotFound_ReturnsFalse()
    {
        // Arrange
        var eventId = 99;
        var updateDto = new UpdateEventDto { Description = "Update" };
        _mockEventRepo.Setup(r => r.GetByIdAsync(eventId)).Returns(Task.FromResult<Event?>(null));

        // Act
        var result = await _service.UpdateAsync(eventId, updateDto);

        // Assert
        Assert.False(result);
        _mockEventRepo.Verify(r => r.GetByIdAsync(eventId), Times.Once);
        _mockEventRepo.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_EventExists_ReturnsTrue()
    {
        // Arrange
        var eventId = 1;
        var existingEvent = new Event { Id = eventId };
        _mockEventRepo.Setup(r => r.GetByIdAsync(eventId)).Returns(Task.FromResult<Event?>(existingEvent));
        _mockEventRepo.Setup(r => r.DeleteAsync(eventId)).Returns(Task.FromResult(true));

        // Act
        var result = await _service.DeleteAsync(eventId);

        // Assert
        Assert.True(result);
        _mockEventRepo.Verify(r => r.GetByIdAsync(eventId), Times.Once);
        _mockEventRepo.Verify(r => r.DeleteAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_EventNotFound_ReturnsFalse()
    {
        // Arrange
        var eventId = 99;
        _mockEventRepo.Setup(r => r.GetByIdAsync(eventId)).Returns(Task.FromResult<Event?>(null));

        // Act
        var result = await _service.DeleteAsync(eventId);

        // Assert
        Assert.False(result);
        _mockEventRepo.Verify(r => r.GetByIdAsync(eventId), Times.Once);
        _mockEventRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
} 