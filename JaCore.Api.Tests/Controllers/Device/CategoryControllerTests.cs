using Xunit;
using Moq;
using JaCore.Api.Controllers.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Dtos.Common; // For PaginatedListDto
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http; // For StatusCodes

namespace JaCore.Api.Tests.Controllers.Device;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly Mock<ILogger<CategoryController>> _mockLogger;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _mockCategoryService = new Mock<ICategoryService>();
        _mockLogger = new Mock<ILogger<CategoryController>>();
        _controller = new CategoryController(_mockCategoryService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkObjectResult_WithPaginatedCategories()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var categories = new List<CategoryDto> { new CategoryDto { Id = 1, Name = "Test" } };
        var paginatedList = new PaginatedListDto<CategoryDto>(categories, 1, pageNumber, pageSize);
        _mockCategoryService.Setup(service => service.GetAllAsync(pageNumber, pageSize)).ReturnsAsync(paginatedList);

        // Act
        var result = await _controller.GetAll(pageNumber, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result); // Need to access .Result for ActionResult<T>
        var returnValue = Assert.IsType<PaginatedListDto<CategoryDto>>(okResult.Value);
        Assert.Single(returnValue.Items);
        _mockCategoryService.Verify(s => s.GetAllAsync(pageNumber, pageSize), Times.Once);
    }
    
    [Fact]
    public async Task GetById_ExistingId_ReturnsOkObjectResult_WithCategory()
    {
        // Arrange
        var testId = 1;
        var categoryDto = new CategoryDto { Id = testId, Name = "Test" };
        _mockCategoryService.Setup(s => s.GetByIdAsync(testId)).ReturnsAsync(categoryDto);
        
        // Act
        var result = await _controller.GetById(testId);
        
        // Assert
         var okResult = Assert.IsType<OkObjectResult>(result.Result);
         var returnValue = Assert.IsType<CategoryDto>(okResult.Value);
         Assert.Equal(testId, returnValue.Id);
         _mockCategoryService.Verify(s => s.GetByIdAsync(testId), Times.Once);
    }
    
    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFoundResult()
    {
        // Arrange
        var testId = 99;
        _mockCategoryService.Setup(s => s.GetByIdAsync(testId)).ReturnsAsync((CategoryDto?)null);
        
        // Act
        var result = await _controller.GetById(testId);
        
        // Assert
         Assert.IsType<NotFoundResult>(result.Result);
         _mockCategoryService.Verify(s => s.GetByIdAsync(testId), Times.Once);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "New Cat" };
        var createdDto = new CategoryDto { Id = 1, Name = createDto.Name };
        _mockCategoryService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(CategoryController.GetById), createdAtActionResult.ActionName);
        Assert.Equal(createdDto.Id, createdAtActionResult.RouteValues!["id"]);
        Assert.Equal(createdDto, createdAtActionResult.Value);
        _mockCategoryService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "" }; // Invalid DTO
        _mockCategoryService.Setup(s => s.CreateAsync(createDto))
                            .ThrowsAsync(new ArgumentException("Name is required", "Name"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.IsAssignableFrom<SerializableError>(badRequestResult.Value); // Check for ModelState errors
        _mockCategoryService.Verify(s => s.CreateAsync(createDto), Times.Once);
    }
    
    [Fact]
    public async Task Delete_ExistingId_ServiceReturnsTrue_ReturnsNoContent()
    {
        // Arrange
        var testId = 1;
        _mockCategoryService.Setup(s => s.DeleteAsync(testId)).ReturnsAsync(true);
        
        // Act
        var result = await _controller.Delete(testId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockCategoryService.Verify(s => s.DeleteAsync(testId), Times.Once);
    }
    
    [Fact]
    public async Task Delete_NonExistingId_ServiceReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        var testId = 99;
        _mockCategoryService.Setup(s => s.DeleteAsync(testId)).ReturnsAsync(false);
        
        // Act
        var result = await _controller.Delete(testId);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockCategoryService.Verify(s => s.DeleteAsync(testId), Times.Once);
    }

    [Fact]
    public async Task Delete_ServiceThrowsInvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        var testId = 1;
        var exceptionMessage = "Cannot delete category with linked devices";
        _mockCategoryService.Setup(s => s.DeleteAsync(testId))
                            .ThrowsAsync(new InvalidOperationException(exceptionMessage));
        
        // Act
        var result = await _controller.Delete(testId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Equal(exceptionMessage, problemDetails.Detail);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        _mockCategoryService.Verify(s => s.DeleteAsync(testId), Times.Once);
    }
} 