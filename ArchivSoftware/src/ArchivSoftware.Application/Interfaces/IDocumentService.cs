using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Application.Interfaces;

/// <summary>
/// Service-Interface für Dokumentenoperationen.
/// </summary>
public interface IDocumentService
{
    Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentDataDto?> GetWithDataAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> GetByFolderAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<DocumentDto> CreateAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default);
    Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithHashAsync(string sha256, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Importiert eine Datei vom Dateisystem und speichert sie als Dokument.
    /// </summary>
    /// <param name="folderId">ID des Zielordners.</param>
    /// <param name="filePath">Pfad zur Datei.</param>
    /// <returns>Das erstellte Dokument.</returns>
    Task<Document> ImportFileAsync(Guid folderId, string filePath, CancellationToken cancellationToken = default);
}
