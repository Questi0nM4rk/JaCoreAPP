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
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;

namespace JaCore.Api.Tests.Controllers.Device;

public class SupplierControllerTests
{
    private readonly Mock<ISupplierService> _mockService;
    private readonly Mock<ILogger<SupplierController>> _mockLogger;
    private readonly SupplierController _controller;

    public SupplierControllerTests()
    {
        _mockService = new Mock<ISupplierService>();
        _mockLogger = new Mock<ILogger<SupplierController>>();
        _controller = new SupplierController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkObjectResult_WithPaginatedSuppliers()
    {
        // Arrange
        var suppliers = new List<SupplierDto> { new SupplierDto { Id = 1 } };
        var paginatedList = new PaginatedListDto<SupplierDto>(suppliers, 1, 1, 10);
        _mockService.Setup(s => s.GetAllAsync(1, 10)).ReturnsAsync(paginatedList);

        // Act
        var result = await _controller.GetAll(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<PaginatedListDto<SupplierDto>>(okResult.Value);
        _mockService.Verify(s => s.GetAllAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkObjectResult()
    {
        // Arrange
        var dto = new SupplierDto { Id = 1 };
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(dto);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFoundResult()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((SupplierDto?)null);

        // Act
        var result = await _controller.GetById(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createDto = new CreateSupplierDto { Name = "New" };
        var createdDto = new SupplierDto { Id = 1, Name = "New" };
        _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Update_ExistingId_ValidDto_ServiceReturnsTrue_ReturnsNoContent()
    {
        // Arrange
        var updateDto = new UpdateSupplierDto { Name = "Update" };
        _mockService.Setup(s => s.UpdateAsync(1, updateDto)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_NonExistingId_ServiceReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateSupplierDto { Name = "Update" };
        _mockService.Setup(s => s.UpdateAsync(99, updateDto)).ReturnsAsync(false);

        // Act
        var result = await _controller.Update(99, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
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
    }
} 