using System.Text;
using ArchivSoftware.Application.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace ArchivSoftware.Infrastructure.Services;

/// <summary>
/// Implementierung der Textextraktion für PDF und DOCX Dateiformate.
/// </summary>
public class TextExtractor : ITextExtractor
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".docx"
    };

    /// <summary>
    /// Extrahiert Text aus einer Datei basierend auf dem Dateiformat.
    /// Unterstützt nur PDF und DOCX.
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
            ".pdf" => ExtractFromPdf(filePath),
            ".docx" => await ExtractFromDocxAsync(filePath),
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

            return NormalizeWhitespace(textBuilder.ToString());
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Extrahiert Text aus einer DOCX-Datei mit OpenXml.
    /// </summary>
    private static Task<string> ExtractFromDocxAsync(string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                var textBuilder = new StringBuilder();

                using var wordDocument = WordprocessingDocument.Open(filePath, false);
                var body = wordDocument.MainDocumentPart?.Document?.Body;

                if (body == null)
                    return string.Empty;

                // Alle Text-Elemente aus dem Dokument extrahieren
                foreach (var text in body.Descendants<Text>())
                {
                    textBuilder.Append(text.Text);
                }

                // Paragraphen-Struktur berücksichtigen
                var result = new StringBuilder();
                foreach (var paragraph in body.Descendants<Paragraph>())
                {
                    var paragraphText = new StringBuilder();
                    foreach (var text in paragraph.Descendants<Text>())
                    {
                        paragraphText.Append(text.Text);
                    }
                    
                    var pText = paragraphText.ToString().Trim();
                    if (!string.IsNullOrEmpty(pText))
                    {
                        result.AppendLine(pText);
                    }
                }

                return NormalizeWhitespace(result.ToString());
            }
            catch
            {
                return string.Empty;
            }
        });
    }

    /// <summary>
    /// Normalisiert Whitespace im extrahierten Text.
    /// </summary>
    private static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Mehrfache Leerzeichen durch einzelne ersetzen
        var normalized = System.Text.RegularExpressions.Regex.Replace(text.Trim(), @"[ \t]+", " ");
        
        // Mehrfache Zeilenumbrüche durch maximal zwei ersetzen
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"(\r?\n){3,}", "\n\n");
        
        return normalized;
    }
}
