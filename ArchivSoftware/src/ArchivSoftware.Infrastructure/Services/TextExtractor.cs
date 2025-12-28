using System.Text;
using ArchivSoftware.Application.Interfaces;
using UglyToad.PdfPig;

namespace ArchivSoftware.Infrastructure.Services;

/// <summary>
/// Implementierung der Textextraktion für verschiedene Dateiformate.
/// </summary>
public class TextExtractor : ITextExtractor
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".pdf"
    };

    /// <summary>
    /// Extrahiert Text aus einer Datei basierend auf dem Dateiformat.
    /// </summary>
    public async Task<string> ExtractAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".txt" => await ExtractFromTextFileAsync(filePath),
            ".pdf" => ExtractFromPdf(filePath),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Prüft ob das Dateiformat für Textextraktion unterstützt wird.
    /// </summary>
    public bool SupportsExtraction(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return SupportedExtensions.Contains(extension);
    }

    /// <summary>
    /// Extrahiert Text aus einer Textdatei.
    /// </summary>
    private static async Task<string> ExtractFromTextFileAsync(string filePath)
    {
        try
        {
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Extrahiert Text aus einer PDF-Datei mit PdfPig.
    /// </summary>
    private static string ExtractFromPdf(string filePath)
    {
        try
        {
            var textBuilder = new StringBuilder();

            using var document = PdfDocument.Open(filePath);
            
            foreach (var page in document.GetPages())
            {
                var pageText = page.Text;
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    textBuilder.AppendLine(pageText);
                }
            }

            return textBuilder.ToString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }
}
