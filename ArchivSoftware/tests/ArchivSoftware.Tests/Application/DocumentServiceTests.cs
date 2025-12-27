using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Services;
using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;
using Moq;
using Xunit;

namespace ArchivSoftware.Tests.Application;

/// <summary>
/// Tests für den DocumentService.
/// </summary>
public class DocumentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly Mock<ITagRepository> _mockTagRepository;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _mockTagRepository = new Mock<ITagRepository>();

        _mockUnitOfWork.Setup(u => u.Documents).Returns(_mockDocumentRepository.Object);
        _mockUnitOfWork.Setup(u => u.Tags).Returns(_mockTagRepository.Object);

        _documentService = new DocumentService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDocument_WhenDocumentExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document
        {
            Id = documentId,
            Title = "Test Document",
            FilePath = "C:\\test.pdf",
            FileType = "pdf",
            FileSize = 1024,
            CreatedAt = DateTime.UtcNow,
            Tags = new List<Tag>()
        };

        _mockDocumentRepository
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _documentService.GetByIdAsync(documentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(documentId, result.Id);
        Assert.Equal("Test Document", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _mockDocumentRepository
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _documentService.GetByIdAsync(documentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDocuments()
    {
        // Arrange
        var documents = new List<Document>
        {
            new() { Id = Guid.NewGuid(), Title = "Doc 1", FilePath = "path1", FileType = "pdf", Tags = new List<Tag>() },
            new() { Id = Guid.NewGuid(), Title = "Doc 2", FilePath = "path2", FileType = "docx", Tags = new List<Tag>() }
        };

        _mockDocumentRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _documentService.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateDocument()
    {
        // Arrange
        var createDto = new CreateDocumentDto(
            "New Document",
            "Description",
            "C:\\new.pdf",
            "pdf",
            2048,
            null,
            null);

        _mockDocumentRepository
            .Setup(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document d, CancellationToken _) => d);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _documentService.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Document", result.Title);
        Assert.Equal("Description", result.Description);
        _mockDocumentRepository.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _mockDocumentRepository
            .Setup(r => r.DeleteAsync(documentId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _documentService.DeleteAsync(documentId);

        // Assert
        _mockDocumentRepository.Verify(r => r.DeleteAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
