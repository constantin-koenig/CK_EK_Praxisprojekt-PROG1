using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;

namespace ArchivSoftware.Application.Services;

/// <summary>
/// Service-Implementierung für Ordneroperationen.
/// </summary>
public class FolderService : IFolderService
{
    private readonly IUnitOfWork _unitOfWork;

    public FolderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FolderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await _unitOfWork.Folders.GetByIdAsync(id, cancellationToken);
        return folder is null ? null : MapToDto(folder);
    }

    public async Task<IEnumerable<FolderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var folders = await _unitOfWork.Folders.GetAllAsync(cancellationToken);
        return folders.Select(MapToDto);
    }

    public async Task<IEnumerable<FolderDto>> GetRootFoldersAsync(CancellationToken cancellationToken = default)
    {
        var folders = await _unitOfWork.Folders.GetRootFoldersAsync(cancellationToken);
        return folders.Select(MapToDto);
    }

    public async Task<IEnumerable<FolderDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var folders = await _unitOfWork.Folders.GetChildrenAsync(parentId, cancellationToken);
        return folders.Select(MapToDto);
    }

    public async Task<IEnumerable<FolderTreeDto>> GetFolderTreeAsync(CancellationToken cancellationToken = default)
    {
        var rootFolders = await _unitOfWork.Folders.GetRootFoldersAsync(cancellationToken);
        var treeDtos = new List<FolderTreeDto>();

        foreach (var folder in rootFolders)
        {
            treeDtos.Add(await BuildTreeDtoAsync(folder, cancellationToken));
        }

        return treeDtos;
    }

    private async Task<FolderTreeDto> BuildTreeDtoAsync(Folder folder, CancellationToken cancellationToken)
    {
        var children = await _unitOfWork.Folders.GetChildrenAsync(folder.Id, cancellationToken);
        var childDtos = new List<FolderTreeDto>();

        foreach (var child in children)
        {
            childDtos.Add(await BuildTreeDtoAsync(child, cancellationToken));
        }

        return new FolderTreeDto(
            folder.Id,
            folder.Name,
            folder.ParentFolderId,
            childDtos,
            folder.Documents.Count,
            folder.CreatedAt);
    }

    public async Task<FolderDto> CreateAsync(CreateFolderDto dto, CancellationToken cancellationToken = default)
    {
        // Prüfe ob Name im selben Parent bereits existiert
        if (await _unitOfWork.Folders.ExistsWithNameAsync(dto.Name, dto.ParentFolderId, cancellationToken))
        {
            throw new InvalidOperationException($"Ein Ordner mit dem Namen '{dto.Name}' existiert bereits in diesem Verzeichnis.");
        }

        var folder = Folder.Create(dto.Name, dto.ParentFolderId);

        await _unitOfWork.Folders.AddAsync(folder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(folder);
    }

    public async Task<FolderDto> UpdateAsync(Guid id, UpdateFolderDto dto, CancellationToken cancellationToken = default)
    {
        var folder = await _unitOfWork.Folders.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {id} wurde nicht gefunden.");

        // Prüfe ob neuer Name im selben Parent bereits existiert
        if (folder.Name != dto.Name && 
            await _unitOfWork.Folders.ExistsWithNameAsync(dto.Name, folder.ParentFolderId, cancellationToken))
        {
            throw new InvalidOperationException($"Ein Ordner mit dem Namen '{dto.Name}' existiert bereits in diesem Verzeichnis.");
        }

        folder.Name = dto.Name;
        folder.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Folders.UpdateAsync(folder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(folder);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await _unitOfWork.Folders.GetWithChildrenAndDocumentsAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {id} wurde nicht gefunden.");

        if (folder.Children.Any())
        {
            throw new InvalidOperationException("Der Ordner kann nicht gelöscht werden, da er Unterordner enthält.");
        }

        if (folder.Documents.Any())
        {
            throw new InvalidOperationException("Der Ordner kann nicht gelöscht werden, da er Dokumente enthält.");
        }

        await _unitOfWork.Folders.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<FolderDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var folders = await _unitOfWork.Folders.SearchByNameAsync(searchTerm, cancellationToken);
        return folders.Select(MapToDto);
    }

    private static FolderDto MapToDto(Folder folder)
    {
        return new FolderDto(
            folder.Id,
            folder.Name,
            folder.ParentFolderId,
            folder.Children.Count,
            folder.Documents.Count,
            folder.CreatedAt);
    }
}
