using Xunit;
using Moq;
using JaCore.Api.Services.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.Device;
using JaCore.Api.Dtos.Device;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JaCore.Api.Dtos.Common;
using System.Linq.Expressions;
using System;
using Microsoft.EntityFrameworkCore;

namespace JaCore.Api.Tests.Services.Device;

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _mockRepo;
    private readonly Mock<ILogger<SupplierService>> _mockLogger;
    private readonly SupplierService _service;

    public SupplierServiceTests()
    {
        _mockRepo = new Mock<ISupplierRepository>();
        _mockLogger = new Mock<ILogger<SupplierService>>();
        _service = new SupplierService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedList()
    {
        // Arrange
        var suppliers = new List<Supplier> { new Supplier { Id = 1, Name = "S1" } };
        _mockRepo.Setup(r => r.CountAsync()).ReturnsAsync(1);
        _mockRepo.Setup(r => r.GetAllAsync(1, 10, It.IsAny<Expression<Func<Supplier, object>>>(), It.IsAny<bool>())).ReturnsAsync(suppliers);

        // Act
        var result = await _service.GetAllAsync(1, 10);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        _mockRepo.Verify(r => r.CountAsync(), Times.Once);
        _mockRepo.Verify(r => r.GetAllAsync(1, 10, It.IsAny<Expression<Func<Supplier, object>>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDto()
    {
        // Arrange
        var supplier = new Supplier { Id = 1, Name = "S1" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(supplier.Id, result.Id);
        Assert.Equal(supplier.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Supplier?)null);

        // Act
        var result = await _service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedDto()
    {
        // Arrange
        var createDto = new CreateSupplierDto { Name = "New S", Contact = "C1" };
        var addedSupplier = new Supplier { Id = 1, Name = createDto.Name, Contact = createDto.Contact };
        _mockRepo.Setup(r => r.AddAsync(It.Is<Supplier>(s => s.Name == createDto.Name))).ReturnsAsync(addedSupplier);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedSupplier.Id, result.Id);
        Assert.Equal(addedSupplier.Name, result.Name);
        Assert.Equal(addedSupplier.Contact, result.Contact);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Supplier>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_InvalidName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var createDto = new CreateSupplierDto { Name = name ?? string.Empty };
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
        Assert.Equal(nameof(createDto.Name), ex.ParamName);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Supplier>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateAsync_ExistingId_ValidDto_ReturnsTrue()
    {
        // Arrange
        var updateDto = new UpdateSupplierDto { Name = "Updated S", Contact = "UC1" };
        var existingSupplier = new Supplier { Id = 1, Name = "Old S" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingSupplier);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Supplier>())).ReturnsAsync((Supplier s) => s); // Return updated entity
        
        // Act
        var result = await _service.UpdateAsync(1, updateDto);
        
        // Assert
        Assert.True(result);
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<Supplier>(s => s.Id == 1 && s.Name == updateDto.Name && s.Contact == updateDto.Contact)), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Supplier?)null);
        
        // Act
        var result = await _service.UpdateAsync(99, new UpdateSupplierDto { Name = "Update" });
        
        // Assert
        Assert.False(result);
         _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Supplier>()), Times.Never);
    }
    
    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var supplier = new Supplier { Id = 1 };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);
        _mockRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);
        // TODO: Add mock for linked entity check if it's added to service

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Supplier?)null);

        // Act
        var result = await _service.DeleteAsync(99);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
} 