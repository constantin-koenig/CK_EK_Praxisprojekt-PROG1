using ArchivSoftware.Application.DTOs;

namespace ArchivSoftware.Application.Interfaces;

/// <summary>
/// Service-Interface für Ordneroperationen.
/// </summary>
public interface IFolderService
{
    Task<FolderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderDto>> GetRootFoldersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderTreeDto>> GetFolderTreeAsync(CancellationToken cancellationToken = default);
    Task<FolderDto> CreateAsync(CreateFolderDto dto, CancellationToken cancellationToken = default);
    Task<FolderDto> UpdateAsync(Guid id, UpdateFolderDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
