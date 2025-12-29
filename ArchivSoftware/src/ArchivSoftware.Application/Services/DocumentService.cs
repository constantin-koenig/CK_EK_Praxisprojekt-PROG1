using System.Diagnostics;
using System.Text;
using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;

namespace ArchivSoftware.Application.Services;

/// <summary>
/// Service-Implementierung für Dokumentenoperationen.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITextExtractor _textExtractor;

    public DocumentService(IUnitOfWork unitOfWork, ITextExtractor textExtractor)
    {
        _unitOfWork = unitOfWork;
        _textExtractor = textExtractor;
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id, cancellationToken);
        return document is null ? null : MapToDto(document);
    }

    public async Task<DocumentDataDto?> GetWithDataAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetWithDataAsync(id, cancellationToken);
        return document is null ? null : MapToDataDto(document);
    }

    public async Task<IEnumerable<DocumentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.GetAllAsync(cancellationToken);
        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<DocumentDto>> GetByFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.GetByFolderIdAsync(folderId, cancellationToken);
        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<DocumentDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.SearchAsync(searchTerm, cancellationToken);
        return documents.Select(MapToDto);
    }

    public async Task<DocumentDto> CreateAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default)
    {
        // Prüfe ob Ordner existiert
        var folder = await _unitOfWork.Folders.GetByIdAsync(dto.FolderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {dto.FolderId} wurde nicht gefunden.");

        var document = Document.Create(
            dto.Title,
            dto.FileName,
            dto.ContentType,
            dto.Data,
            dto.FolderId,
            dto.PlainText);

        // Prüfe auf Duplikate anhand des Hash
        if (document.Sha256 != null && await _unitOfWork.Documents.ExistsWithHashAsync(document.Sha256, cancellationToken))
        {
            throw new InvalidOperationException("Ein Dokument mit identischem Inhalt existiert bereits.");
        }

        await _unitOfWork.Documents.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        document.Folder = folder;
        return MapToDto(document);
    }

    public async Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Dokument mit ID {id} wurde nicht gefunden.");

        document.Title = dto.Title;
        document.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(document);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Documents.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithHashAsync(string sha256, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Documents.ExistsWithHashAsync(sha256, cancellationToken);
    }

    public async Task MoveToFolderAsync(Guid documentId, Guid newFolderId, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Dokument mit ID {documentId} wurde nicht gefunden.");

        var newFolder = await _unitOfWork.Folders.GetByIdAsync(newFolderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Zielordner mit ID {newFolderId} wurde nicht gefunden.");

        document.FolderId = newFolderId;
        document.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Exportiert ein Dokument an einen angegebenen Pfad.
    /// </summary>
    public async Task ExportToFileAsync(Guid documentId, string targetPath, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetWithDataAsync(documentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Dokument mit ID {documentId} wurde nicht gefunden.");

        // Stelle sicher, dass das Zielverzeichnis existiert
        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(targetPath, document.Data, cancellationToken);
    }

    /// <summary>
    /// Öffnet ein Dokument mit der Standardanwendung.
    /// Exportiert das Dokument in ein temporäres Verzeichnis und startet den zugehörigen Prozess.
    /// </summary>
    /// <returns>Der Pfad zur temporären Datei.</returns>
    public async Task<string> OpenDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetWithDataAsync(documentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Dokument mit ID {documentId} wurde nicht gefunden.");

        // Erstelle temporäre Datei mit eindeutigem Namen
        var tempDirectory = Path.Combine(Path.GetTempPath(), "ArchivSoftware");
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        var tempPath = Path.Combine(tempDirectory, $"{document.Id}_{document.FileName}");
        
        // Schreibe Datei
        await File.WriteAllBytesAsync(tempPath, document.Data, cancellationToken);

        // Öffne mit Standardanwendung
        var processStartInfo = new ProcessStartInfo
        {
            FileName = tempPath,
            UseShellExecute = true
        };
        Process.Start(processStartInfo);

        return tempPath;
    }

    /// <summary>
    /// Importiert eine Datei vom Dateisystem und speichert sie als Dokument.
    /// Nur PDF und DOCX Dateien sind erlaubt.
    /// </summary>
    /// <exception cref="NotSupportedException">Wenn der Dateityp nicht erlaubt ist.</exception>
    public async Task<Document> ImportFileAsync(Guid folderId, string filePath, CancellationToken cancellationToken = default)
    {
        // Prüfe ob Dateityp erlaubt ist
        if (!FileTypePolicy.IsAllowed(filePath))
        {
            throw new NotSupportedException(FileTypePolicy.NotAllowedMessage);
        }

        // Prüfe ob Datei existiert
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Die Datei wurde nicht gefunden: {filePath}");
        }

        // Prüfe ob Ordner existiert
        var folder = await _unitOfWork.Folders.GetByIdAsync(folderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {folderId} wurde nicht gefunden.");

        // Lese Datei-Bytes
        var data = await File.ReadAllBytesAsync(filePath, cancellationToken);

        // Extrahiere Dateiname und Extension
        var fileName = Path.GetFileName(filePath);
        var title = Path.GetFileNameWithoutExtension(filePath);

        // Bestimme ContentType über zentrale Policy
        var contentType = FileTypePolicy.GetContentType(filePath);

        // Extrahiere Text mit ITextExtractor
        var plainText = await _textExtractor.ExtractAsync(filePath);

        // Erstelle Document
        var document = Document.Create(title, fileName, contentType, data, folderId, plainText);

        // Prüfe auf Duplikate anhand des Hash
        if (document.Sha256 != null && await _unitOfWork.Documents.ExistsWithHashAsync(document.Sha256, cancellationToken))
        {
            throw new InvalidOperationException("Ein Dokument mit identischem Inhalt existiert bereits.");
        }

        // Speichere über Repository
        await _unitOfWork.Documents.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        document.Folder = folder;
        return document;
    }

    /// <summary>
    /// (Nicht mehr verwendet - ContentType wird über FileTypePolicy ermittelt)
    /// </summary>
    [Obsolete("Use FileTypePolicy.GetContentType instead")]
    private static string GetContentType(string extension)
    {
        return FileTypePolicy.GetContentType($".{extension.TrimStart('.')}");
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto(
            document.Id,
            document.Title,
            document.FileName,
            document.ContentType,
            document.Data.Length,
            document.Sha256,
            document.FolderId,
            document.Folder?.Name,
            document.CreatedAt);
    }

    private static DocumentDataDto MapToDataDto(Document document)
    {
        return new DocumentDataDto(
            document.Id,
            document.Title,
            document.FileName,
            document.ContentType,
            document.Data,
            document.PlainText,
            document.Sha256,
            document.FolderId,
            document.CreatedAt);
    }
}
