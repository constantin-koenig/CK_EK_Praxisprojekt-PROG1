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

    public DocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

    /// <summary>
    /// Importiert eine Datei vom Dateisystem und speichert sie als Dokument.
    /// </summary>
    public async Task<Document> ImportFileAsync(Guid folderId, string filePath, CancellationToken cancellationToken = default)
    {
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
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        // Bestimme ContentType basierend auf Extension
        var contentType = GetContentType(extension);

        // PlainText: nur bei .txt Dateien
        string plainText = string.Empty;
        if (extension == ".txt")
        {
            plainText = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
        }

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
    /// Gibt den ContentType basierend auf der Dateiendung zurück.
    /// </summary>
    private static string GetContentType(string extension)
    {
        return extension switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".xml" => "application/xml",
            ".json" => "application/json",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".mp3" => "audio/mpeg",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".csv" => "text/csv",
            ".rtf" => "application/rtf",
            _ => "application/octet-stream"
        };
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
