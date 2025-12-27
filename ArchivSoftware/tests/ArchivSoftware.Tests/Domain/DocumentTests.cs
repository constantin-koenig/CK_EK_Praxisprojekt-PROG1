using ArchivSoftware.Domain.Entities;
using Xunit;

namespace ArchivSoftware.Tests.Domain;

/// <summary>
/// Tests für die Document-Entity.
/// </summary>
public class DocumentTests
{
    [Fact]
    public void Document_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var document = new Document();

        // Assert
        Assert.Equal(string.Empty, document.Title);
        Assert.Equal(string.Empty, document.FilePath);
        Assert.Equal(string.Empty, document.FileType);
        Assert.Null(document.Description);
        Assert.Null(document.CategoryId);
        Assert.NotNull(document.Tags);
        Assert.Empty(document.Tags);
    }

    [Fact]
    public void Document_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Document";
        var description = "Test Description";
        var filePath = "C:\\test\\document.pdf";
        var fileType = "pdf";
        var fileSize = 1024L;
        var categoryId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var document = new Document
        {
            Id = id,
            Title = title,
            Description = description,
            FilePath = filePath,
            FileType = fileType,
            FileSize = fileSize,
            CategoryId = categoryId,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(id, document.Id);
        Assert.Equal(title, document.Title);
        Assert.Equal(description, document.Description);
        Assert.Equal(filePath, document.FilePath);
        Assert.Equal(fileType, document.FileType);
        Assert.Equal(fileSize, document.FileSize);
        Assert.Equal(categoryId, document.CategoryId);
        Assert.Equal(createdAt, document.CreatedAt);
    }

    [Fact]
    public void Document_ShouldAllowAddingTags()
    {
        // Arrange
        var document = new Document();
        var tag = new Tag { Id = Guid.NewGuid(), Name = "Important" };

        // Act
        document.Tags.Add(tag);

        // Assert
        Assert.Single(document.Tags);
        Assert.Contains(tag, document.Tags);
    }
}
