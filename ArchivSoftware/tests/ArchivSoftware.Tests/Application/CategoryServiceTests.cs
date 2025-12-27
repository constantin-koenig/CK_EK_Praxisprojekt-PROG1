using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Services;
using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;
using Moq;
using Xunit;

namespace ArchivSoftware.Tests.Application;

/// <summary>
/// Tests für den CategoryService.
/// </summary>
public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();

        _mockUnitOfWork.Setup(u => u.Categories).Returns(_mockCategoryRepository.Object);

        _categoryService = new CategoryService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            CreatedAt = DateTime.UtcNow,
            Documents = new List<Document>()
        };

        _mockCategoryRepository
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetByIdAsync(categoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.Id);
        Assert.Equal("Test Category", result.Name);
    }

    [Fact]
    public async Task GetRootCategoriesAsync_ShouldReturnOnlyRootCategories()
    {
        // Arrange
        var rootCategories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Root 1", Documents = new List<Document>() },
            new() { Id = Guid.NewGuid(), Name = "Root 2", Documents = new List<Document>() }
        };

        _mockCategoryRepository
            .Setup(r => r.GetRootCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rootCategories);

        // Act
        var result = await _categoryService.GetRootCategoriesAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCategory()
    {
        // Arrange
        var createDto = new CreateCategoryDto("New Category", "Description", null);

        _mockCategoryRepository
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category c, CancellationToken _) => c);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Category", result.Name);
        _mockCategoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto("Updated", null, null);

        _mockCategoryRepository
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _categoryService.UpdateAsync(categoryId, updateDto));
    }
}
