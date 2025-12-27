using ArchivSoftware.Domain.Entities;
using Xunit;

namespace ArchivSoftware.Tests.Domain;

/// <summary>
/// Tests für die Category-Entity.
/// </summary>
public class CategoryTests
{
    [Fact]
    public void Category_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var category = new Category();

        // Assert
        Assert.Equal(string.Empty, category.Name);
        Assert.Null(category.Description);
        Assert.Null(category.ParentCategoryId);
        Assert.NotNull(category.SubCategories);
        Assert.Empty(category.SubCategories);
        Assert.NotNull(category.Documents);
        Assert.Empty(category.Documents);
    }

    [Fact]
    public void Category_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Category";
        var description = "Test Description";
        var parentId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var category = new Category
        {
            Id = id,
            Name = name,
            Description = description,
            ParentCategoryId = parentId,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(id, category.Id);
        Assert.Equal(name, category.Name);
        Assert.Equal(description, category.Description);
        Assert.Equal(parentId, category.ParentCategoryId);
        Assert.Equal(createdAt, category.CreatedAt);
    }

    [Fact]
    public void Category_ShouldAllowAddingSubCategories()
    {
        // Arrange
        var parentCategory = new Category { Id = Guid.NewGuid(), Name = "Parent" };
        var subCategory = new Category 
        { 
            Id = Guid.NewGuid(), 
            Name = "Child",
            ParentCategoryId = parentCategory.Id,
            ParentCategory = parentCategory
        };

        // Act
        parentCategory.SubCategories.Add(subCategory);

        // Assert
        Assert.Single(parentCategory.SubCategories);
        Assert.Contains(subCategory, parentCategory.SubCategories);
        Assert.Equal(parentCategory.Id, subCategory.ParentCategoryId);
    }

    [Fact]
    public void Category_ShouldAllowAddingDocuments()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category" };
        var document = new Document 
        { 
            Id = Guid.NewGuid(), 
            Title = "Test Document",
            CategoryId = category.Id
        };

        // Act
        category.Documents.Add(document);

        // Assert
        Assert.Single(category.Documents);
        Assert.Contains(document, category.Documents);
    }
}
