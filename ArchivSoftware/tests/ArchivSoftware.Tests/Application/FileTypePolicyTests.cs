using Xunit;

namespace ArchivSoftware.Tests.Application;

/// <summary>
/// Unit Tests für die FileTypePolicy.
/// </summary>
public class FileTypePolicyTests
{
    [Theory]
    [InlineData("document.pdf", true)]
    [InlineData("document.PDF", true)]
    [InlineData("document.Pdf", true)]
    [InlineData("document.docx", true)]
    [InlineData("document.DOCX", true)]
    [InlineData("document.Docx", true)]
    public void IsAllowed_WithAllowedExtension_ReturnsTrue(string fileName, bool expected)
    {
        // Act
        var result = ArchivSoftware.Application.FileTypePolicy.IsAllowed(fileName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("document.txt")]
    [InlineData("document.doc")]
    [InlineData("document.xlsx")]
    [InlineData("document.jpg")]
    [InlineData("document.png")]
    [InlineData("document")]
    [InlineData("")]
    [InlineData(null)]
    public void IsAllowed_WithNotAllowedExtension_ReturnsFalse(string? fileName)
    {
        // Act
        var result = ArchivSoftware.Application.FileTypePolicy.IsAllowed(fileName!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetContentType_ForPdf_ReturnsCorrectMimeType()
    {
        // Act
        var result = ArchivSoftware.Application.FileTypePolicy.GetContentType("document.pdf");

        // Assert
        Assert.Equal("application/pdf", result);
    }

    [Fact]
    public void GetContentType_ForDocx_ReturnsCorrectMimeType()
    {
        // Act
        var result = ArchivSoftware.Application.FileTypePolicy.GetContentType("document.docx");

        // Assert
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", result);
    }

    [Fact]
    public void GetContentType_ForUnknown_ReturnsOctetStream()
    {
        // Act
        var result = ArchivSoftware.Application.FileTypePolicy.GetContentType("document.xyz");

        // Assert
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void AllowedExtensions_ContainsOnlyPdfAndDocx()
    {
        // Assert
        Assert.Equal(2, ArchivSoftware.Application.FileTypePolicy.AllowedExtensions.Length);
        Assert.Contains(".pdf", ArchivSoftware.Application.FileTypePolicy.AllowedExtensions);
        Assert.Contains(".docx", ArchivSoftware.Application.FileTypePolicy.AllowedExtensions);
        Assert.DoesNotContain(".txt", ArchivSoftware.Application.FileTypePolicy.AllowedExtensions);
    }

    [Fact]
    public void OpenFileDialogFilter_ContainsPdfAndDocx()
    {
        // Assert
        Assert.Contains(".pdf", ArchivSoftware.Application.FileTypePolicy.OpenFileDialogFilter);
        Assert.Contains(".docx", ArchivSoftware.Application.FileTypePolicy.OpenFileDialogFilter);
        Assert.DoesNotContain(".txt", ArchivSoftware.Application.FileTypePolicy.OpenFileDialogFilter);
    }
}
