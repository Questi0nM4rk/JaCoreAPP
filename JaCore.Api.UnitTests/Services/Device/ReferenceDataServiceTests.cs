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
using AutoMapper;

namespace JaCore.Api.UnitTests.Services.Device;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockRepo;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ILogger<CategoryService>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _mockRepo = new Mock<ICategoryRepository>();
        _mockLogger = new Mock<ILogger<CategoryService>>();
        _mockMapper = new Mock<IMapper>();
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new CategoryService(_mockRepo.Object, _mockDbContext.Object, _mockLogger.Object, _mockMapper.Object);
    }

    // Minimal tests - expand as needed

    [Fact]
    public async Task GetCategoryByIdAsync_WhenExists_ReturnsDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Test" };
        _mockRepo.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Category, object>>[]>()))
                 .ReturnsAsync(category);

        // Act
        var result = await _sut.GetCategoryByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenNameIsUnique_ReturnsDto()
    {
        // Arrange
        var dto = new CategoryCreateDto("New Category", null);
        var categoryId = Guid.NewGuid();
        _mockRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                 .Callback<Category, CancellationToken>((cat, ct) => cat.Id = categoryId);

        // Act
        var result = await _sut.CreateCategoryAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Name.Should().Be(dto.Name);
        _mockRepo.Verify(r => r.AddAsync(It.Is<Category>(c => c.Name == dto.Name), It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Add tests for GetAll, Update, Delete, duplicate name scenarios...
}

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _mockRepo;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ILogger<SupplierService>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly SupplierService _sut;

    public SupplierServiceTests()
    {
        _mockRepo = new Mock<ISupplierRepository>();
        _mockLogger = new Mock<ILogger<SupplierService>>();
        _mockMapper = new Mock<IMapper>();
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new SupplierService(_mockRepo.Object, _mockDbContext.Object, _mockLogger.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetSupplierByIdAsync_WhenExists_ReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Supplier { Id = id, Name = "Sup1" };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Supplier, object>>[]>())).ReturnsAsync(entity);
        // Act
        var result = await _sut.GetSupplierByIdAsync(id);
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }
    // Add tests for Create, GetAll, Update, Delete...
}

public class ServiceServiceTests // Renamed class
{
    private readonly Mock<IServiceRepository> _mockRepo;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<ILogger<ServiceService>> _mockLogger; // Correct Logger type
    private readonly Mock<IMapper> _mockMapper;
    private readonly ServiceService _sut; // Renamed service

    public ServiceServiceTests()
    {
        _mockRepo = new Mock<IServiceRepository>();
        _mockLogger = new Mock<ILogger<ServiceService>>();
        _mockMapper = new Mock<IMapper>();
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new ServiceService(_mockRepo.Object, _mockDbContext.Object, _mockLogger.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenExists_ReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Entities.Device.Service { Id = id, Name = "Svc1", ProviderName = "Prov1" };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Entities.Device.Service, object>>[]>())).ReturnsAsync(entity);
        // Act
        var result = await _sut.GetServiceByIdAsync(id);
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }
    // Add tests for Create, GetAll, Update, Delete...
} 