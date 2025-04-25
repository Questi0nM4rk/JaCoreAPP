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

namespace JaCore.Api.UnitTests.Services.Device;

public class DeviceServiceTests
{
    private readonly Mock<IDeviceRepository> _mockDeviceRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly Mock<ISupplierRepository> _mockSupplierRepo;
    private readonly Mock<ApplicationDbContext> _mockDbContext; // Mock DbContext for SaveChanges
    private readonly Mock<ILogger<DeviceService>> _mockLogger;
    private readonly DeviceService _sut; // System Under Test

    public DeviceServiceTests()
    {
        _mockDeviceRepo = new Mock<IDeviceRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _mockSupplierRepo = new Mock<ISupplierRepository>();

        // Mock DbContext - Simple setup for SaveChangesAsync
        // For more complex scenarios involving ChangeTracker, a more elaborate setup or
        // an in-memory provider might be needed, but for basic SaveChanges check, this is often sufficient.
        var options = new DbContextOptions<ApplicationDbContext>(); // Dummy options
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1); // Assume 1 record saved successfully

        _mockLogger = new Mock<ILogger<DeviceService>>();

        // Instantiate the service with mocks
        _sut = new DeviceService(
            _mockDeviceRepo.Object,
            _mockCategoryRepo.Object,
            _mockSupplierRepo.Object,
            _mockDbContext.Object, // Use mocked context
            _mockLogger.Object
        );
    }

    // Helper method to capture log messages (optional but useful)
    private void VerifyLog<TException>(LogLevel level, string messageContains, Times times) where TException : Exception
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageContains)),
                It.IsAny<TException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            times);
    }
     private void VerifyLog(LogLevel level, string messageContains, Times times)
    {
         VerifyLog<Exception>(level, messageContains, times);
    }


    // --- Test Cases --- 

    #region GetDeviceByIdAsync Tests

    [Fact]
    public async Task GetDeviceByIdAsync_WhenDeviceExists_ShouldReturnDeviceReadDto()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category" };
        var supplier = new Supplier { Id = Guid.NewGuid(), Name = "Test Supplier" };
        var deviceEntity = new Entities.Device.Device
        {
            Id = deviceId,
            Name = "Test Device",
            SerialNumber = "SN123",
            CategoryId = category.Id,
            SupplierId = supplier.Id,
            Category = category, // Include navigation props for mapping
            Supplier = supplier,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedAt = DateTimeOffset.UtcNow
        };

        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), 
                                                         It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(deviceEntity);

        // Act
        var result = await _sut.GetDeviceByIdAsync(deviceId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<DeviceReadDto>();
        result!.Id.Should().Be(deviceId);
        result.Name.Should().Be(deviceEntity.Name);
        result.SerialNumber.Should().Be(deviceEntity.SerialNumber);
        result.CategoryName.Should().Be(category.Name);
        result.SupplierName.Should().Be(supplier.Name);
        _mockDeviceRepo.Verify(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), 
                                                        It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()), Times.Once);
    }

    [Fact]
    public async Task GetDeviceByIdAsync_WhenDeviceDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>(),
                                                         It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync((Entities.Device.Device?)null);

        // Act
        var result = await _sut.GetDeviceByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
         _mockDeviceRepo.Verify(repo => repo.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>(),
                                                        It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()), Times.Once);
    }

    [Fact]
    public async Task GetDeviceByIdAsync_WhenRepositoryThrowsException_ShouldReturnNullAndLogError()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var exception = new InvalidOperationException("Database connection lost");
        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ThrowsAsync(exception);

        // Act
        var result = await _sut.GetDeviceByIdAsync(deviceId);

        // Assert
        result.Should().BeNull();
        VerifyLog(LogLevel.Error, $"Error retrieving device with ID {deviceId}", Times.Once());
        // Optionally verify the exception type if needed
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error retrieving device with ID {deviceId}")),
                It.Is<InvalidOperationException>(ex => ex == exception), // Check for specific exception instance
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    #endregion

     #region GetAllDevicesAsync Tests

    [Fact]
    public async Task GetAllDevicesAsync_WhenDevicesExist_ShouldReturnDeviceReadDtoList()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Cat 1" };
        var supplier = new Supplier { Id = Guid.NewGuid(), Name = "Sup 1" };
        var devices = new List<Entities.Device.Device>
        {
            new Entities.Device.Device { Id = Guid.NewGuid(), Name = "Dev1", SerialNumber = "SN1", Category = category, Supplier = supplier },
            new Entities.Device.Device { Id = Guid.NewGuid(), Name = "Dev2", SerialNumber = "SN2", Category = category, Supplier = supplier }
        };

        _mockDeviceRepo.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>(),
                                                     It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(devices);

        // Act
        var result = await _sut.GetAllDevicesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Dev1");
        result.Last().CategoryName.Should().Be("Cat 1");
        _mockDeviceRepo.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>(),
                                                        It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()), Times.Once);
    }

     [Fact]
    public async Task GetAllDevicesAsync_WhenNoDevicesExist_ShouldReturnEmptyList()
    {
        // Arrange
        _mockDeviceRepo.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>(),
                                                     It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(new List<Entities.Device.Device>());

        // Act
        var result = await _sut.GetAllDevicesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllDevicesAsync_WhenRepositoryThrowsException_ShouldReturnEmptyListAndLogError()
    {
        // Arrange
        var exception = new Exception("Repo failure");
        _mockDeviceRepo.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ThrowsAsync(exception);

        // Act
        var result = await _sut.GetAllDevicesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        VerifyLog(LogLevel.Error, "Error retrieving all devices", Times.Once());
    }

    #endregion

    #region CreateDeviceAsync Tests

    [Fact]
    public async Task CreateDeviceAsync_WithValidDataAndUniqueSerial_ShouldReturnCreatedDeviceDto()
    {
        // Arrange
        var createDto = new DeviceCreateDto("New Device", "UniqueSN", "ModelX", "ManuY", null, null, Guid.NewGuid(), Guid.NewGuid());
        var deviceId = Guid.NewGuid();
        var createdEntity = new Entities.Device.Device { Id = deviceId, Name = createDto.Name, SerialNumber = createDto.SerialNumber /* ... other props */ };

        // Mock setup: Serial does not exist, Add succeeds, SaveChanges succeeds
        _mockDeviceRepo.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false); // Serial does not exist
        _mockDeviceRepo.Setup(repo => repo.AddAsync(It.IsAny<Entities.Device.Device>(), It.IsAny<CancellationToken>()))
                       .Callback<Entities.Device.Device, CancellationToken>((dev, ct) => dev.Id = deviceId); // Assign ID on Add
        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(createdEntity); // Mock the refetch call
        // SaveChangesAsync is mocked in constructor to return 1

        // Act
        var result = await _sut.CreateDeviceAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(deviceId);
        result.Name.Should().Be(createDto.Name);
        result.SerialNumber.Should().Be(createDto.SerialNumber);

        _mockDeviceRepo.Verify(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDeviceRepo.Verify(repo => repo.AddAsync(It.Is<Entities.Device.Device>(d => d.Name == createDto.Name && d.SerialNumber == createDto.SerialNumber), It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockDeviceRepo.Verify(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()), Times.Once); // Verify refetch
    }

    [Fact]
    public async Task CreateDeviceAsync_WithDuplicateSerial_ShouldReturnNullAndLogWarning()
    {
        // Arrange
        var createDto = new DeviceCreateDto("New Device", "DuplicateSN", "ModelX", "ManuY", null, null, null, null);

        _mockDeviceRepo.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true); // Serial *does* exist

        // Act
        var result = await _sut.CreateDeviceAsync(createDto);

        // Assert
        result.Should().BeNull();
        VerifyLog(LogLevel.Warning, "Device creation failed: Serial number", Times.Once());
        _mockDeviceRepo.Verify(repo => repo.AddAsync(It.IsAny<Entities.Device.Device>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

     [Fact]
    public async Task CreateDeviceAsync_WhenSaveChangesFails_ShouldReturnNullAndLogError()
    {
        // Arrange
         var createDto = new DeviceCreateDto("New Device", "UniqueSN", "ModelX", "ManuY", null, null, Guid.NewGuid(), Guid.NewGuid());
        _mockDeviceRepo.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(0); // Simulate SaveChanges failure

        // Act
        var result = await _sut.CreateDeviceAsync(createDto);

        // Assert
        result.Should().BeNull();
        VerifyLog(LogLevel.Error, "Failed to save new device", Times.Once());
         _mockDeviceRepo.Verify(repo => repo.AddAsync(It.IsAny<Entities.Device.Device>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDeviceAsync_WhenAddAsyncThrows_ShouldReturnNullAndLogError()
    {
        // Arrange
        var createDto = new DeviceCreateDto("Error Device", "SNERR", null, null, null, null, null, null);
        var exception = new InvalidOperationException("Cannot add");

        _mockDeviceRepo.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);
        _mockDeviceRepo.Setup(repo => repo.AddAsync(It.IsAny<Entities.Device.Device>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(exception);

        // Act
        var result = await _sut.CreateDeviceAsync(createDto);

        // Assert
        result.Should().BeNull();
        // Verify the log from the generic catch block in CreateDeviceAsync
        VerifyLog(LogLevel.Error, "Error during device creation process", Times.Once());
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); // Save shouldn't be called
    }

    [Fact]
    public async Task CreateDeviceAsync_WhenRefetchGetByIdAsyncThrows_ShouldReturnNullAndLogError()
    {
        // Arrange
        var createDto = new DeviceCreateDto("Refetch Fail", "SNRF", null, null, null, null, null, null);
        var deviceId = Guid.NewGuid();
        var refetchException = new Exception("Refetch failed");

        _mockDeviceRepo.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false); // Serial does not exist
        _mockDeviceRepo.Setup(repo => repo.AddAsync(It.IsAny<Entities.Device.Device>(), It.IsAny<CancellationToken>()))
                       .Callback<Entities.Device.Device, CancellationToken>((dev, ct) => dev.Id = deviceId); // Assign ID on Add
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1); // SaveChanges succeeds
        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ThrowsAsync(refetchException); // Mock the refetch call to throw

        // Act
        var result = await _sut.CreateDeviceAsync(createDto);

        // Assert
        // Even though saved, the method fails to return the DTO due to refetch error
        result.Should().BeNull();
        // Verify the log from the catch block within the nested GetDeviceByIdAsync call
        VerifyLog(LogLevel.Error, $"Error retrieving device with ID", Times.Once()); 
        _mockDeviceRepo.Verify(repo => repo.AddAsync(It.IsAny<Entities.Device.Device>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockDeviceRepo.Verify(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()), Times.Once);
    }

    #endregion

    #region UpdateDeviceAsync Tests

    [Fact]
    public async Task UpdateDeviceAsync_WhenDeviceExistsAndSaveChangesSucceeds_ShouldReturnTrue()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var updateDto = new DeviceUpdateDto("Updated Name", "ModelY", "ManuZ", DateTimeOffset.UtcNow, null, Guid.NewGuid(), Guid.NewGuid());
        var existingDevice = new Entities.Device.Device { Id = deviceId, Name = "Old Name", SerialNumber = "SN123" };

        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>())) // Update GetByIdAsync mock
                       .ReturnsAsync(existingDevice);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateDeviceAsync(deviceId, updateDto);

        // Assert
        result.Should().BeTrue();
        _mockDeviceRepo.Verify(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()), Times.Once); // Verify GetByIdAsync call
        _mockDeviceRepo.Verify(repo => repo.Update(It.Is<Entities.Device.Device>(d => 
            d.Id == deviceId &&
            d.Name == updateDto.Name &&
            d.ModelNumber == updateDto.ModelNumber &&
            d.Manufacturer == updateDto.Manufacturer &&
            d.CategoryId == updateDto.CategoryId &&
            d.SupplierId == updateDto.SupplierId
        )), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDeviceAsync_WhenDeviceDoesNotExist_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateDto = new DeviceUpdateDto("Updated Name", null, null, null, null, null, null);

        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>())) // Update GetByIdAsync mock
                       .ReturnsAsync((Entities.Device.Device?)null);

        // Act
        var result = await _sut.UpdateDeviceAsync(nonExistentId, updateDto);

        // Assert
        result.Should().BeFalse();
        VerifyLog(LogLevel.Warning, "Device update failed", Times.Once());
        _mockDeviceRepo.Verify(repo => repo.Update(It.IsAny<Entities.Device.Device>()), Times.Never);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

     [Fact]
    public async Task UpdateDeviceAsync_WhenSaveChangesFails_ShouldReturnFalseAndLogError()
    {
        // Arrange
         var deviceId = Guid.NewGuid();
        var updateDto = new DeviceUpdateDto("Updated Name", "ModelY", "ManuZ", DateTimeOffset.UtcNow, null, Guid.NewGuid(), Guid.NewGuid());
        var existingDevice = new Entities.Device.Device { Id = deviceId, Name = "Old Name", SerialNumber = "SN123" };

        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>())) // Update GetByIdAsync mock
                       .ReturnsAsync(existingDevice);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0); // Simulate failure

        // Act
        var result = await _sut.UpdateDeviceAsync(deviceId, updateDto);

        // Assert
        result.Should().BeFalse();
         VerifyLog(LogLevel.Error, "Failed to save update for device", Times.Once());
        _mockDeviceRepo.Verify(repo => repo.Update(It.IsAny<Entities.Device.Device>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDeviceAsync_WhenConcurrencyIssueOccurs_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var updateDto = new DeviceUpdateDto("Concurrent Update", null, null, null, null, null, null);
        var existingDevice = new Entities.Device.Device { Id = deviceId, Name = "Original" };
        var concurrencyException = new DbUpdateConcurrencyException("Concurrency conflict");

        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(existingDevice);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ThrowsAsync(concurrencyException);

        // Act
        var result = await _sut.UpdateDeviceAsync(deviceId, updateDto);

        // Assert
        result.Should().BeFalse();
        VerifyLog<DbUpdateConcurrencyException>(LogLevel.Error, "Error saving changes to the database.", Times.Once());
        _mockDeviceRepo.Verify(repo => repo.Update(existingDevice), Times.Once);
    }

    #endregion

    #region DeleteDeviceAsync Tests

    [Fact]
    public async Task DeleteDeviceAsync_WhenDeviceExistsAndSaveChangesSucceeds_ShouldReturnTrue()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var existingDevice = new Entities.Device.Device { Id = deviceId, Name = "DeviceToDelete" };

        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>())) // Update GetByIdAsync mock
                       .ReturnsAsync(existingDevice);
         _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteDeviceAsync(deviceId);

        // Assert
        result.Should().BeTrue();
         _mockDeviceRepo.Verify(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()), Times.Once); // Verify GetByIdAsync call
        _mockDeviceRepo.Verify(repo => repo.Remove(existingDevice), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDeviceAsync_WhenDeviceDoesNotExist_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>())) // Update GetByIdAsync mock
                       .ReturnsAsync((Entities.Device.Device?)null);

        // Act
        var result = await _sut.DeleteDeviceAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
        VerifyLog(LogLevel.Warning, "Device deletion failed", Times.Once());
        _mockDeviceRepo.Verify(repo => repo.Remove(It.IsAny<Entities.Device.Device>()), Times.Never);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteDeviceAsync_WhenSaveChangesFails_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var existingDevice = new Entities.Device.Device { Id = deviceId, Name = "DeviceToDelete" };

        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>())) // Update GetByIdAsync mock
                       .ReturnsAsync(existingDevice);
         _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0); // Simulate failure

        // Act
        var result = await _sut.DeleteDeviceAsync(deviceId);

        // Assert
        result.Should().BeFalse();
        // Expect the specific log message from the DeleteDeviceAsync method
        VerifyLog(LogLevel.Error, "Failed to save deletion for device", Times.Once()); 
        _mockDeviceRepo.Verify(repo => repo.Remove(existingDevice), Times.Once);
        // SaveChangesAsync is still called once, even if it fails (returns 0)
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDeviceAsync_WhenConcurrencyIssueOccurs_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var existingDevice = new Entities.Device.Device { Id = deviceId, Name = "ToDeleteConcurrent" };
        var concurrencyException = new DbUpdateConcurrencyException("Concurrency conflict on delete");

        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(existingDevice);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ThrowsAsync(concurrencyException);

        // Act
        var result = await _sut.DeleteDeviceAsync(deviceId);

        // Assert
        result.Should().BeFalse();
        VerifyLog<DbUpdateConcurrencyException>(LogLevel.Error, "Error saving changes to the database.", Times.Once());
        _mockDeviceRepo.Verify(repo => repo.Remove(existingDevice), Times.Once);
    }

    [Fact]
    public async Task DeleteDeviceAsync_WhenDbUpdateExceptionOccurs_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var existingDevice = new Entities.Device.Device { Id = deviceId, Name = "ToDeleteConstraint" };
        // Simulate a foreign key constraint violation, for example
        var dbUpdateException = new DbUpdateException("FK constraint violation", new Exception());

        _mockDeviceRepo.Setup(repo => repo.GetByIdAsync(deviceId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(existingDevice);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ThrowsAsync(dbUpdateException);

        // Act
        var result = await _sut.DeleteDeviceAsync(deviceId);

        // Assert
        result.Should().BeFalse();
        // The existing handler already catches DbUpdateException and logs Error
        VerifyLog<DbUpdateException>(LogLevel.Error, "Error saving changes to the database.", Times.Once());
        _mockDeviceRepo.Verify(repo => repo.Remove(existingDevice), Times.Once);
    }

    #endregion

    #region GetDeviceBySerialNumberAsync Tests

    [Fact]
    public async Task GetDeviceBySerialNumberAsync_WhenDeviceExists_ShouldReturnDeviceReadDto()
    {
        // Arrange
        var serialNumber = "SN456";
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category" };
        var supplier = new Supplier { Id = Guid.NewGuid(), Name = "Test Supplier" };
        var deviceEntity = new Entities.Device.Device
        {
            Id = Guid.NewGuid(),
            Name = "Serial Device",
            SerialNumber = serialNumber,
            Category = category, Supplier = supplier // Include for mapping
            // ... other props
        };
        var deviceList = new List<Entities.Device.Device> { deviceEntity };

        _mockDeviceRepo.Setup(repo => repo.FindAsync(
                                   It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(), 
                                   It.IsAny<CancellationToken>(), 
                                   It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(deviceList);

        // Act
        var result = await _sut.GetDeviceBySerialNumberAsync(serialNumber);

        // Assert
        result.Should().NotBeNull();
        result!.SerialNumber.Should().Be(serialNumber);
        result.Name.Should().Be(deviceEntity.Name);
        result.CategoryName.Should().Be(category.Name);
        result.SupplierName.Should().Be(supplier.Name);
         _mockDeviceRepo.Verify(repo => repo.FindAsync(It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()), Times.Once);
    }

    [Fact]
    public async Task GetDeviceBySerialNumberAsync_WhenDeviceDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var serialNumber = "NonExistentSN";
         _mockDeviceRepo.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Entities.Device.Device, bool>>>(expr => expr.ToString().Contains(serialNumber)), 
                                                  It.IsAny<CancellationToken>(), 
                                                  It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(new List<Entities.Device.Device>()); // Return empty list

        // Act
        var result = await _sut.GetDeviceBySerialNumberAsync(serialNumber);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDeviceBySerialNumberAsync_WhenMultipleDevicesExist_ShouldReturnFirstDeviceDto()
    {
        // Arrange
        var serialNumber = "SN_MULTI";
        var category = new Category { Id = Guid.NewGuid(), Name = "CatMulti" };
        var supplier = new Supplier { Id = Guid.NewGuid(), Name = "SupMulti" };
        var deviceList = new List<Entities.Device.Device>
        {
            new Entities.Device.Device { Id = Guid.NewGuid(), Name = "MultiDev1", SerialNumber = serialNumber, Category = category, Supplier = supplier },
            new Entities.Device.Device { Id = Guid.NewGuid(), Name = "MultiDev2", SerialNumber = serialNumber, Category = category, Supplier = supplier }
        };

        _mockDeviceRepo.Setup(repo => repo.FindAsync(
                                   It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(),
                                   It.IsAny<CancellationToken>(),
                                   It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ReturnsAsync(deviceList); // Return multiple devices

        // Act
        var result = await _sut.GetDeviceBySerialNumberAsync(serialNumber);

        // Assert
        result.Should().NotBeNull();
        result!.SerialNumber.Should().Be(serialNumber);
        result.Name.Should().Be("MultiDev1"); // Should be the first one based on current implementation (FirstOrDefault)
    }

    [Fact]
    public async Task GetDeviceBySerialNumberAsync_WhenRepositoryThrowsException_ShouldReturnNullAndLogError()
    {
        // Arrange
        var serialNumber = "SN_EXC";
        var exception = new TimeoutException("Query timed out");
         _mockDeviceRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Entities.Device.Device, bool>>>(),
                                                  It.IsAny<CancellationToken>(),
                                                  It.IsAny<Expression<Func<Entities.Device.Device, object>>[]>()))
                       .ThrowsAsync(exception);

        // Act
        var result = await _sut.GetDeviceBySerialNumberAsync(serialNumber);

        // Assert
        result.Should().BeNull();
        VerifyLog(LogLevel.Error, $"Error retrieving device with serial number {serialNumber}", Times.Once());
    }

    #endregion

} 