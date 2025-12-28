using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;

namespace ArchivSoftware.Application.Services;

/// <summary>
/// Service für die Dokumentensuche mit Snippet-Generierung.
/// </summary>
public class SearchService : ISearchService
{
    private readonly IDocumentRepository _documentRepository;

    public SearchService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    /// <inheritdoc />
    public async Task<List<SearchResultDto>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return new List<SearchResultDto>();
        }

        var documents = await _documentRepository.SearchAsync(term.Trim(), cancellationToken);
        var results = new List<SearchResultDto>();

        foreach (var doc in documents)
        {
            var snippet = GenerateSnippet(doc.PlainText, term);
            var folderPath = BuildFolderPath(doc.Folder);

            results.Add(new SearchResultDto(
                DocumentId: doc.Id,
                Title: doc.Title,
                FileName: doc.FileName,
                FolderId: doc.FolderId,
                FolderPath: folderPath,
                Snippet: snippet
            ));
        }

        return results;
    }

    /// <summary>
    /// Generiert ein Snippet aus dem PlainText mit dem Suchbegriff im Kontext.
    /// </summary>
    private static string GenerateSnippet(string? plainText, string term)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        // Case-insensitive Suche nach dem ersten Vorkommen
        var index = plainText.IndexOf(term, StringComparison.OrdinalIgnoreCase);
        
        if (index < 0)
        {
            // Suchbegriff nicht im PlainText gefunden, zeige Anfang des Textes
            var previewLength = Math.Min(200, plainText.Length);
            var preview = plainText.Substring(0, previewLength);
            return NormalizeWhitespace(preview) + (plainText.Length > previewLength ? "..." : "");
        }

        // 80 Zeichen vor und 120 Zeichen nach dem Treffer
        const int charsBefore = 80;
        const int charsAfter = 120;

        var start = Math.Max(0, index - charsBefore);
        var end = Math.Min(plainText.Length, index + term.Length + charsAfter);

        var snippet = plainText.Substring(start, end - start);

        // Prefix "..." wenn nicht am Anfang
        var prefix = start > 0 ? "..." : "";
        // Suffix "..." wenn nicht am Ende
        var suffix = end < plainText.Length ? "..." : "";

        return prefix + NormalizeWhitespace(snippet) + suffix;
    }

    /// <summary>
    /// Ersetzt Zeilenumbrüche und mehrfache Leerzeichen durch einzelne Leerzeichen.
    /// </summary>
    private static string NormalizeWhitespace(string text)
    {
        // Ersetze Zeilenumbrüche durch Leerzeichen
        var normalized = text
            .Replace("\r\n", " ")
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ");

        // Mehrfache Leerzeichen zu einem reduzieren
        while (normalized.Contains("  "))
        {
            normalized = normalized.Replace("  ", " ");
        }

        return normalized.Trim();
    }

    /// <summary>
    /// Baut den vollständigen Ordnerpfad aus der Folder-Hierarchie.
    /// </summary>
    private static string BuildFolderPath(Folder? folder)
    {
        if (folder == null)
        {
            return string.Empty;
        }

        var pathParts = new List<string>();
        var current = folder;

        while (current != null)
        {
            pathParts.Insert(0, current.Name);
            current = current.ParentFolder;
        }

        return string.Join(" / ", pathParts);
    }
}
