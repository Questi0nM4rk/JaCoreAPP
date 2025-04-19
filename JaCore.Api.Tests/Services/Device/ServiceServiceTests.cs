using Xunit;
using Moq;
using JaCore.Api.Services.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Models.Device;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Linq.Expressions;

namespace JaCore.Api.Tests.Services.Device;

public class ServiceServiceTests
{
    private readonly Mock<IServiceRepository> _mockRepo;
    private readonly Mock<ILogger<ServiceService>> _mockLogger;
    private readonly ServiceService _service;

    public ServiceServiceTests()
    {
        _mockRepo = new Mock<IServiceRepository>();
        _mockLogger = new Mock<ILogger<ServiceService>>();
        _service = new ServiceService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedListDto()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var services = new List<Service> { new Service { Id = 1 } };
        _mockRepo.Setup(r => r.GetAllAsync(pageNumber, pageSize, null, It.IsAny<bool>()))
                 .Returns(Task.FromResult<IEnumerable<Service>>(services));

        // Act
        var result = await _service.GetAllAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        _mockRepo.Verify(r => r.GetAllAsync(pageNumber, pageSize, null, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsServiceDto()
    {
        // Arrange
        var service = new Service { Id = 1 };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(service);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Service?)null);

        // Act
        var result = await _service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedDto()
    {
        // Arrange
        var createDto = new CreateServiceDto { Name = "New Service", Contact = "Contact Info" };
        var addedService = new Service { Id = 1, Name = createDto.Name, Contact = createDto.Contact }; // Simulate the repo adding the service and assigning an ID

        _mockRepo.Setup(repo => repo.AddAsync(It.Is<Service>(s =>
            s.Name == createDto.Name &&
            s.Contact == createDto.Contact
            ))).ReturnsAsync(addedService); // Ensure a valid object is returned

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedService.Id, result.Id);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.Contact, result.Contact);
        _mockRepo.Verify(repo => repo.AddAsync(It.Is<Service>(s => s.Name == createDto.Name && s.Contact == createDto.Contact)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ValidDto_ReturnsTrue()
    {
        // Arrange
        var updateDto = new UpdateServiceDto { Name = "Updated Service" };
        var existingService = new Service { Id = 1, Name = "Old Service" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).Returns(Task.FromResult<Service?>(existingService));
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Service>())).Returns(Task.FromResult(existingService));

        // Act
        var result = await _service.UpdateAsync(1, updateDto);

        // Assert
        Assert.True(result);
        _mockRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<Service>(s => s.Id == 1 && s.Name == "Updated Service")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var updateDto = new UpdateServiceDto { Name = "Updated Service" };
        _mockRepo.Setup(r => r.GetByIdAsync(99)).Returns(Task.FromResult<Service?>(null));

        // Act
        var result = await _service.UpdateAsync(99, updateDto);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.GetByIdAsync(99), Times.Once);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var serviceId = 1;
        var existingService = new Service { Id = serviceId };
        _mockRepo.Setup(r => r.GetByIdAsync(serviceId)).Returns(Task.FromResult<Service?>(existingService));
        _mockRepo.Setup(r => r.DeleteAsync(serviceId)).Returns(Task.FromResult(true));

        // Act
        var result = await _service.DeleteAsync(serviceId);

        // Assert
        Assert.True(result);
        _mockRepo.Verify(r => r.GetByIdAsync(serviceId), Times.Once);
        _mockRepo.Verify(r => r.DeleteAsync(serviceId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var serviceId = 99;
        _mockRepo.Setup(r => r.GetByIdAsync(serviceId)).Returns(Task.FromResult<Service?>(null));

        // Act
        var result = await _service.DeleteAsync(serviceId);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.GetByIdAsync(serviceId), Times.Once);
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
} 