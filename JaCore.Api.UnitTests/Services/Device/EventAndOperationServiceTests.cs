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
using JaCore.Common.Device;

namespace JaCore.Api.UnitTests.Services.Device;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _mockRepo;
    private readonly Mock<IDeviceCardRepository> _mockCardRepo;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ILogger<EventService>> _mockLogger;
    private readonly EventService _sut;

    public EventServiceTests()
    {
        _mockRepo = new Mock<IEventRepository>();
        _mockCardRepo = new Mock<IDeviceCardRepository>();
        _mockLogger = new Mock<ILogger<EventService>>();
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new EventService(_mockRepo.Object, _mockCardRepo.Object, _mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenExists_ReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Event { Id = id, Description = "Test Event", DeviceCardId = Guid.NewGuid() };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Event, object>>[]>())).ReturnsAsync(entity);
        // Act
        var result = await _sut.GetEventByIdAsync(id);
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task CreateEventAsync_WhenDeviceCardExists_ReturnsDto()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var dto = new EventCreateDto(cardId, EventType.Maintenance, DateTimeOffset.UtcNow, "Desc", null, null);
        var eventId = Guid.NewGuid();
        _mockCardRepo.Setup(r => r.ExistsAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                 .Callback<Event, CancellationToken>((e, ct) => e.Id = eventId);
        
        // Act
        var result = await _sut.CreateEventAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(eventId);
        result.DeviceCardId.Should().Be(cardId);
        _mockRepo.Verify(r=> r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Add tests for GetByCardId, Delete, non-existent card...
}

public class DeviceOperationServiceTests
{
    private readonly Mock<IDeviceOperationRepository> _mockRepo;
    private readonly Mock<IDeviceCardRepository> _mockCardRepo;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ILogger<DeviceOperationService>> _mockLogger;
    private readonly DeviceOperationService _sut;

     public DeviceOperationServiceTests()
    {
        _mockRepo = new Mock<IDeviceOperationRepository>();
        _mockCardRepo = new Mock<IDeviceCardRepository>();
        _mockLogger = new Mock<ILogger<DeviceOperationService>>();
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new DeviceOperationService(_mockRepo.Object, _mockCardRepo.Object, _mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetOperationByIdAsync_WhenExists_ReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new DeviceOperation { Id = id, OperationType = "Test Op", DeviceCardId = Guid.NewGuid(), Status="Done" };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<DeviceOperation, object>>[]>())).ReturnsAsync(entity);
        // Act
        var result = await _sut.GetOperationByIdAsync(id);
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

     [Fact]
    public async Task CreateOperationAsync_WhenDeviceCardExists_ReturnsDto()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var dto = new DeviceOperationCreateDto(cardId, "Install X", DateTimeOffset.UtcNow, null, "Started", null, null);
        var opId = Guid.NewGuid();
        _mockCardRepo.Setup(r => r.ExistsAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<DeviceOperation>(), It.IsAny<CancellationToken>()))
                 .Callback<DeviceOperation, CancellationToken>((op, ct) => op.Id = opId);
        
        // Act
        var result = await _sut.CreateOperationAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(opId);
        result.DeviceCardId.Should().Be(cardId);
        _mockRepo.Verify(r=> r.AddAsync(It.IsAny<DeviceOperation>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Add tests for GetByCardId, Update, Delete, non-existent card...
} 