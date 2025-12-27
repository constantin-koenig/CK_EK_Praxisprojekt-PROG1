using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Domain.Interfaces;

/// <summary>
/// Repository-Interface für Dokumente mit CRUD und Suchoperationen.
/// </summary>
public interface IDocumentRepository : IRepository<Document>
{
    /// <summary>
    /// Gibt alle Dokumente in einem Ordner zurück.
    /// </summary>
    Task<IEnumerable<Document>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sucht Dokumente nach Titel.
    /// </summary>
    Task<IEnumerable<Document>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sucht Dokumente nach Inhalt (PlainText).
    /// </summary>
    Task<IEnumerable<Document>> SearchByContentAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sucht Dokumente nach Titel oder Inhalt.
    /// </summary>
    Task<IEnumerable<Document>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gibt ein Dokument anhand seines SHA256-Hashes zurück.
    /// </summary>
    Task<Document?> GetBySha256Async(string sha256, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prüft, ob ein Dokument mit dem angegebenen Hash bereits existiert.
    /// </summary>
    Task<bool> ExistsWithHashAsync(string sha256, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gibt ein Dokument mit seinen Daten zurück.
    /// </summary>
    Task<Document?> GetWithDataAsync(Guid id, CancellationToken cancellationToken = default);
}
