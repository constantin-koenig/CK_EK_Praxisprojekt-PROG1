using ArchivSoftware.Application.DTOs;

namespace ArchivSoftware.Application.Interfaces;

/// <summary>
/// Service-Interface für Dokumentenoperationen.
/// </summary>
public interface IDocumentService
{
    Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<DocumentDto> CreateAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default);
    Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
