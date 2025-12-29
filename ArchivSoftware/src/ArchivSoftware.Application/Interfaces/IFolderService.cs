using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Application.Interfaces;

/// <summary>
/// Service-Interface für Ordneroperationen.
/// </summary>
public interface IFolderService
{
    Task EnsureRootFolderExistsAsync(CancellationToken cancellationToken = default);
    Task<Guid> EnsureSpecialFolderExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Folder>> GetFolderTreeAsync(CancellationToken cancellationToken = default);
    Task<Folder> CreateFolderAsync(Guid parentFolderId, string name, CancellationToken cancellationToken = default);
    Task<FolderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderDto>> GetRootFoldersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderTreeDto>> GetFolderTreeDtosAsync(CancellationToken cancellationToken = default);
    Task<FolderDto> CreateAsync(CreateFolderDto dto, CancellationToken cancellationToken = default);
    Task<FolderDto> UpdateAsync(Guid id, UpdateFolderDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Benennt einen Ordner um. Validiert Namen und prüft Uniqueness pro Parent.
    /// </summary>
    Task RenameFolderAsync(Guid folderId, string newName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Löscht einen Ordner inklusive aller Unterordner und Dokumente (Cascade).
    /// </summary>
    Task DeleteFolderAsync(Guid folderId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verschiebt einen Ordner zu einem neuen Parent. Prüft auf Zyklen und Uniqueness.
    /// </summary>
    Task MoveFolderAsync(Guid folderId, Guid newParentId, CancellationToken cancellationToken = default);
}
