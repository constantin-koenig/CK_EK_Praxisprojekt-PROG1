using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Domain.Interfaces;

/// <summary>
/// Repository-Interface für Dokumente mit spezifischen Abfragen.
/// </summary>
public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetByTagAsync(string tagName, CancellationToken cancellationToken = default);
}
