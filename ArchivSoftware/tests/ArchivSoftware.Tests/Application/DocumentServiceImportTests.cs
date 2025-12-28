using Moq;
using Xunit;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Application.Services;
using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;

namespace ArchivSoftware.Tests.Application;

/// <summary>
/// Unit Tests für DocumentService.ImportFileAsync mit Fokus auf Dateityp-Validierung.
/// </summary>
public class DocumentServiceImportTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITextExtractor> _mockTextExtractor;
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly Mock<IFolderRepository> _mockFolderRepository;
    private readonly DocumentService _documentService;

    public DocumentServiceImportTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTextExtractor = new Mock<ITextExtractor>();
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _mockFolderRepository = new Mock<IFolderRepository>();

        _mockUnitOfWork.Setup(u => u.Documents).Returns(_mockDocumentRepository.Object);
        _mockUnitOfWork.Setup(u => u.Folders).Returns(_mockFolderRepository.Object);

        _documentService = new DocumentService(_mockUnitOfWork.Object, _mockTextExtractor.Object);
    }

    [Theory]
    [InlineData("document.txt")]
    [InlineData("document.doc")]
    [InlineData("document.xlsx")]
    [InlineData("document.jpg")]
    [InlineData("document.xml")]
    public async Task ImportFileAsync_WithNotAllowedFileType_ThrowsNotSupportedException(string fileName)
    {
        // Arrange
        var folderId = Guid.NewGuid();
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _documentService.ImportFileAsync(folderId, filePath));

        Assert.Equal(ArchivSoftware.Application.FileTypePolicy.NotAllowedMessage, exception.Message);
    }

    [Fact]
    public async Task ImportFileAsync_WithTxtFile_ThrowsNotSupportedException()
    {
        // Arrange
        var folderId = Guid.NewGuid();
        var filePath = Path.Combine(Path.GetTempPath(), "test.txt");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _documentService.ImportFileAsync(folderId, filePath));

        Assert.Contains("PDF", exception.Message);
        Assert.Contains("DOCX", exception.Message);
    }

    [Fact]
    public async Task ImportFileAsync_WithPdfFile_DoesNotThrowNotSupportedException()
    {
        // Arrange
        var folderId = Guid.NewGuid();
        var tempDir = Path.GetTempPath();
        var filePath = Path.Combine(tempDir, $"test_{Guid.NewGuid()}.pdf");
        
        // Erstelle temporäre PDF-Datei (minimal valid PDF)
        await File.WriteAllBytesAsync(filePath, GetMinimalPdfBytes());

        try
        {
            var folder = Folder.Create("TestFolder", null);
            typeof(Folder).GetProperty("Id")!.SetValue(folder, folderId);

            _mockFolderRepository.Setup(r => r.GetByIdAsync(folderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(folder);
            _mockTextExtractor.Setup(t => t.ExtractAsync(It.IsAny<string>()))
                .ReturnsAsync("Test content");
            _mockDocumentRepository.Setup(r => r.ExistsWithHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockDocumentRepository.Setup(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Document doc, CancellationToken _) => doc);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _documentService.ImportFileAsync(folderId, filePath);

            // Assert
            Assert.NotNull(result);
            Assert.EndsWith(".pdf", result.FileName);
        }
        finally
        {
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ImportFileAsync_WithDocxFile_DoesNotThrowNotSupportedException()
    {
        // Arrange
        var folderId = Guid.NewGuid();
        var tempDir = Path.GetTempPath();
        var filePath = Path.Combine(tempDir, $"test_{Guid.NewGuid()}.docx");
        
        // Erstelle temporäre DOCX-Datei (minimal valid DOCX - es ist ein ZIP)
        await File.WriteAllBytesAsync(filePath, GetMinimalDocxBytes());

        try
        {
            var folder = Folder.Create("TestFolder", null);
            typeof(Folder).GetProperty("Id")!.SetValue(folder, folderId);

            _mockFolderRepository.Setup(r => r.GetByIdAsync(folderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(folder);
            _mockTextExtractor.Setup(t => t.ExtractAsync(It.IsAny<string>()))
                .ReturnsAsync("Test content");
            _mockDocumentRepository.Setup(r => r.ExistsWithHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockDocumentRepository.Setup(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Document doc, CancellationToken _) => doc);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _documentService.ImportFileAsync(folderId, filePath);

            // Assert
            Assert.NotNull(result);
            Assert.EndsWith(".docx", result.FileName);
        }
        finally
        {
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    /// <summary>
    /// Minimale gültige PDF-Bytes für Testzwecke.
    /// </summary>
    private static byte[] GetMinimalPdfBytes()
    {
        // Minimales gültiges PDF
        var pdfContent = @"%PDF-1.4
1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj
2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj
3 0 obj<</Type/Page/MediaBox[0 0 612 792]/Parent 2 0 R>>endobj
xref
0 4
0000000000 65535 f 
0000000009 00000 n 
0000000052 00000 n 
0000000101 00000 n 
trailer<</Size 4/Root 1 0 R>>
startxref
166
%%EOF";
        return System.Text.Encoding.ASCII.GetBytes(pdfContent);
    }

    /// <summary>
    /// Minimale DOCX-Bytes (leeres ZIP für Testzwecke).
    /// </summary>
    private static byte[] GetMinimalDocxBytes()
    {
        // Minimales ZIP-Archiv (DOCX ist ein ZIP)
        return new byte[]
        {
            0x50, 0x4B, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
    }
}
