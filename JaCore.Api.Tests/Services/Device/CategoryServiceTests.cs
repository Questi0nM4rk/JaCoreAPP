using Xunit;
using Moq;
using JaCore.Api.Services.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Models.Device;
using JaCore.Api.Dtos.Device;
// using JaCore.ApiO.Data; // No longer needed for DbContext mock
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JaCore.Api.Dtos.Common; // For PaginatedListDto
using System.Linq.Expressions; // For mocking GetAllAsync
using System;

namespace JaCore.Api.Tests.Services.Device;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    // private readonly Mock<ApplicationDbContext> _mockDbContext; // Removed
    private readonly Mock<ILogger<CategoryService>> _mockLogger;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockLogger = new Mock<ILogger<CategoryService>>();

        _categoryService = new CategoryService(
            _mockCategoryRepository.Object,
            // _mockDbContext.Object, // Removed
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedListOfCategoryDtos()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var categories = new List<Category> 
        { 
            new Category { Id = 1, Name = "Test 1" }, 
            new Category { Id = 2, Name = "Test 2" } 
        };
        _mockCategoryRepository.Setup(repo => repo.CountAsync()).ReturnsAsync(categories.Count);
        _mockCategoryRepository.Setup(repo => repo.GetAllAsync(pageNumber, pageSize, It.IsAny<Expression<Func<Category, object>>>(), It.IsAny<bool>()))
                             .ReturnsAsync(categories);

        // Act
        var result = await _categoryService.GetAllAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PaginatedListDto<CategoryDto>>(result);
        Assert.Equal(categories.Count, result.TotalCount);
        Assert.Equal(categories.Count, result.Items.Count());
        Assert.Equal(categories[0].Name, result.Items.First().Name);
        _mockCategoryRepository.Verify(repo => repo.CountAsync(), Times.Once); 
        _mockCategoryRepository.Verify(repo => repo.GetAllAsync(pageNumber, pageSize, It.IsAny<Expression<Func<Category, object>>>(), It.IsAny<bool>()), Times.Once); 
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCategoryDto()
    {
        // Arrange
        var testId = 1;
        var category = new Category { Id = testId, Name = "Test Category" };
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(testId)).ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetByIdAsync(testId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testId, result.Id);
        Assert.Equal(category.Name, result.Name);
        _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(testId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var testId = 99;
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(testId)).ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetByIdAsync(testId);

        // Assert
        Assert.Null(result);
        _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(testId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedCategoryDto()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "New Category" };
        var createdCategory = new Category { Id = 5, Name = createDto.Name }; // Simulate ID generation
        _mockCategoryRepository.Setup(repo => repo.AddAsync(It.IsAny<Category>()))
                             .ReturnsAsync(createdCategory);

        // Act
        var result = await _categoryService.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdCategory.Id, result.Id);
        Assert.Equal(createDto.Name, result.Name);
        _mockCategoryRepository.Verify(repo => repo.AddAsync(It.Is<Category>(c => c.Name == createDto.Name)), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Too long name.....................................................................................................")] // > 100 chars
    public async Task CreateAsync_InvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = invalidName ?? string.Empty };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.CreateAsync(createDto));
        Assert.Equal(nameof(createDto.Name), exception.ParamName);
        _mockCategoryRepository.Verify(repo => repo.AddAsync(It.IsAny<Category>()), Times.Never);
    }
    
    [Fact]
    public async Task DeleteAsync_CategoryExists_NoLinkedDevices_ReturnsTrue()
    {
        // Arrange
        var testId = 1;
        var category = new Category { Id = testId, Name = "ToDelete" };
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(testId)).ReturnsAsync(category);
        _mockCategoryRepository.Setup(repo => repo.DeleteAsync(testId)).Returns(Task.CompletedTask);
        _mockCategoryRepository.Setup(repo => repo.HasLinkedDevicesAsync(testId)).ReturnsAsync(false); // Mock repository check

        // Act
        var result = await _categoryService.DeleteAsync(testId);

        // Assert
        Assert.True(result);
        _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(testId), Times.Once);
        _mockCategoryRepository.Verify(repo => repo.HasLinkedDevicesAsync(testId), Times.Once); // Verify new check
        _mockCategoryRepository.Verify(repo => repo.DeleteAsync(testId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CategoryExists_HasLinkedDevices_ThrowsInvalidOperationException()
    {
        // Arrange
        var testId = 1;
        var category = new Category { Id = testId, Name = "ToDelete" };
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(testId)).ReturnsAsync(category);
        _mockCategoryRepository.Setup(repo => repo.HasLinkedDevicesAsync(testId)).ReturnsAsync(true); // Mock repository check

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _categoryService.DeleteAsync(testId));
        _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(testId), Times.Once);
        _mockCategoryRepository.Verify(repo => repo.HasLinkedDevicesAsync(testId), Times.Once); // Verify new check
        _mockCategoryRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_CategoryNotFound_ReturnsFalse()
    {
        // Arrange
        var testId = 99;
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(testId)).ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.DeleteAsync(testId);

        // Assert
        Assert.False(result);
        _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(testId), Times.Once);
        _mockCategoryRepository.Verify(repo => repo.HasLinkedDevicesAsync(It.IsAny<int>()), Times.Never); // Should not be called
        _mockCategoryRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}

// --- Async Mocking Helpers removed as they are no longer needed for these tests --- 