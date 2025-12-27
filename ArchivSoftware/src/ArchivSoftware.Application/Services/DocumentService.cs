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
