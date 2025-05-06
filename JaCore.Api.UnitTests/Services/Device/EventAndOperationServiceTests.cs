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
using AutoMapper;

namespace JaCore.Api.UnitTests.Services.Device;

public class EventAndOperationServiceTests
{
    private readonly Mock<IDeviceEventRepository> _mockEventRepo;
    private readonly Mock<IDeviceCardRepository> _mockCardRepo;
    private readonly Mock<IDeviceOperationRepository> _mockOpRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<DeviceEventService>> _mockEventLogger;
    private readonly Mock<ILogger<DeviceOperationService>> _mockOpLogger;
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IDeviceRepository> _mockDeviceRepo;

    private readonly DeviceEventService _eventService;
    private readonly DeviceOperationService _operationService;

    public EventAndOperationServiceTests()
    {
        _mockEventRepo = new Mock<IDeviceEventRepository>();
        _mockCardRepo = new Mock<IDeviceCardRepository>();
        _mockOpRepo = new Mock<IDeviceOperationRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockEventLogger = new Mock<ILogger<DeviceEventService>>();
        _mockOpLogger = new Mock<ILogger<DeviceOperationService>>();
        _mockDeviceRepo = new Mock<IDeviceRepository>();

        var options = new DbContextOptions<ApplicationDbContext>();
        _mockContext = new Mock<ApplicationDbContext>(options);

        _eventService = new DeviceEventService(
            _mockEventRepo.Object,
            _mockCardRepo.Object, 
            _mockContext.Object,
            _mockEventLogger.Object,
            _mockMapper.Object
        );

        _operationService = new DeviceOperationService(
            _mockOpRepo.Object,
            _mockDeviceRepo.Object,
            _mockContext.Object,
            _mockOpLogger.Object,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenExists_ReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new DeviceEvent { Id = id, Description = "Test Event", CardId = Guid.NewGuid() };
        _mockEventRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entity);
        // Act
        var result = await _eventService.GetEventByIdAsync(id);
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task CreateEventAsync_ShouldReturnDto_WhenCardExistsAndSaveSucceeds()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var createDto = new EventCreateDto(cardId, EventType.Connected, DateTimeOffset.UtcNow, "Test Event", null, "TestUser");
        var eventEntity = new DeviceEvent { Id = Guid.NewGuid(), CardId = cardId };
        var eventReadDto = new EventReadDto(eventEntity.Id, cardId, EventType.Connected, DateTimeOffset.UtcNow, "Test Event", null, "TestUser", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        _mockCardRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<DeviceCard, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<DeviceEvent>(createDto)).Returns(eventEntity);
        _mockMapper.Setup(m => m.Map<EventReadDto>(eventEntity)).Returns(eventReadDto);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _eventService.CreateEventAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(eventReadDto);
        _mockEventRepo.Verify(r => r.AddAsync(eventEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOperationByIdAsync_WhenExists_ReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new DeviceOperation { Id = id, OperationType = "Test Op", DeviceCardId = Guid.NewGuid(), Status="Done" };
        _mockOpRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(entity);
        // Act
        var result = await _operationService.GetOperationByIdAsync(id);
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
        _mockOpRepo.Setup(r => r.AddAsync(It.IsAny<DeviceOperation>(), It.IsAny<CancellationToken>()))
                 .Callback<DeviceOperation, CancellationToken>((op, ct) => op.Id = opId);
        
        // Act
        var result = await _operationService.CreateOperationAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(opId);
        result.DeviceCardId.Should().Be(cardId);
        _mockOpRepo.Verify(r=> r.AddAsync(It.IsAny<DeviceOperation>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Add tests for GetByCardId, Update, Delete, non-existent card...

    [Fact]
    public void SomeTest()
    {
        // Arrange
        var mockRepository = new Mock<IDeviceRepository>();
        var service = new EventAndOperationService(mockRepository.Object);
        var eventType = EventType.Unknown;

        // Act
        // ... Add test logic here ...

        // Assert
        // ... Add assertions here ...

        Assert.True(true); // Placeholder assertion
    }
} 