using Xunit;
using Moq;
using JaCore.Api.Controllers.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Dtos.Common;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JaCore.Api.Tests.Controllers.Device;

public class ServiceControllerTests
{
    private readonly Mock<IServiceService> _mockService;
    private readonly Mock<ILogger<ServiceController>> _mockLogger;
    private readonly ServiceController _controller;

    public ServiceControllerTests()
    {
        _mockService = new Mock<IServiceService>();
        _mockLogger = new Mock<ILogger<ServiceController>>();
        _controller = new ServiceController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkObjectResult_WithPaginatedListDto()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var items = new List<ServiceDto> { new ServiceDto { Id = 1, Name = "Service1" } };
        var paginatedDto = new PaginatedListDto<ServiceDto>(items, 1, pageNumber, pageSize);
        _mockService.Setup(s => s.GetAllAsync(pageNumber, pageSize)).ReturnsAsync(paginatedDto);

        // Act
        var result = await _controller.GetAll(pageNumber, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsAssignableFrom<PaginatedListDto<ServiceDto>>(okResult.Value);
        Assert.Single(returnedDto.Items);
        _mockService.Verify(s => s.GetAllAsync(pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkObjectResult()
    {
        // Arrange
        var serviceDto = new ServiceDto { Id = 1, Name = "Test Service" };
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(serviceDto);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ServiceDto>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
        Assert.Equal("Test Service", returnValue.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFoundResult()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((ServiceDto?)null);

        // Act
        var result = await _controller.GetById(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createDto = new CreateServiceDto { Name = "New Service" };
        var createdDto = new ServiceDto { Id = 1, Name = "New Service" };
        _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal("GetById", createdAtActionResult.ActionName);
        Assert.Equal(1, createdAtActionResult.RouteValues!["id"]);
        Assert.Equal(createdDto, createdAtActionResult.Value);
    }

    [Fact]
    public async Task Update_ExistingId_ValidDto_ServiceReturnsTrue_ReturnsNoContent()
    {
        // Arrange
        var updateDto = new UpdateServiceDto { Name = "Updated Service" };
        _mockService.Setup(s => s.UpdateAsync(1, updateDto)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.UpdateAsync(1, updateDto), Times.Once);
    }

    [Fact]
    public async Task Update_NonExistingId_ServiceReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateServiceDto { Name = "Updated Service" };
        _mockService.Setup(s => s.UpdateAsync(99, updateDto)).ReturnsAsync(false);

        // Act
        var result = await _controller.Update(99, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.UpdateAsync(99, updateDto), Times.Once);
    }

    [Fact]
    public async Task Delete_ExistingId_ServiceReturnsTrue_ReturnsNoContent()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistingId_ServiceReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(s => s.DeleteAsync(99), Times.Once);
    }
} 