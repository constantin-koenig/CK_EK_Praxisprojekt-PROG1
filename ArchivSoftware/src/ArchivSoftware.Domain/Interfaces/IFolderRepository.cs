using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Domain.Interfaces;

/// <summary>
/// Repository-Interface für Ordner mit CRUD und Suchoperationen.
/// </summary>
public interface IFolderRepository : IRepository<Folder>
{
    /// <summary>
    /// Gibt alle Root-Ordner zurück (ohne Parent).
    /// </summary>
    Task<IEnumerable<Folder>> GetRootFoldersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gibt alle Unterordner eines Ordners zurück.
    /// </summary>
    Task<IEnumerable<Folder>> GetChildrenAsync(Guid parentFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gibt einen Ordner mit allen Unterordnern und Dokumenten zurück.
    /// </summary>
    Task<Folder?> GetWithChildrenAndDocumentsAsync(Guid folderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sucht Ordner nach Name.
    /// </summary>
    Task<IEnumerable<Folder>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prüft, ob ein Ordnername im selben Parent-Ordner bereits existiert.
    /// </summary>
    Task<bool> ExistsWithNameAsync(string name, Guid? parentFolderId, CancellationToken cancellationToken = default);
}
