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

    /// <summary>
    /// Stellt sicher, dass ein Root-Ordner existiert. Legt "Root" an, wenn keiner existiert.
    /// </summary>
    public async Task EnsureRootFolderExistsAsync(CancellationToken cancellationToken = default)
    {
        var rootFolders = await _unitOfWork.Folders.GetRootFoldersAsync(cancellationToken);
        
        if (!rootFolders.Any())
        {
            var rootFolder = Folder.Create("Root", null);
            await _unitOfWork.Folders.AddAsync(rootFolder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Stellt sicher, dass ein spezieller Ordner unter Root existiert.
    /// Legt ihn an, falls er fehlt, und gibt dessen ID zurück.
    /// </summary>
    /// <param name="name">Name des speziellen Ordners.</param>
    /// <returns>Die ID des Ordners.</returns>
    public async Task<Guid> EnsureSpecialFolderExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        // Stelle sicher, dass Root existiert
        await EnsureRootFolderExistsAsync(cancellationToken);

        // Hole Root-Ordner
        var rootFolders = await _unitOfWork.Folders.GetRootFoldersAsync(cancellationToken);
        var rootFolder = rootFolders.First();

        // Prüfe, ob der spezielle Ordner bereits existiert
        var children = await _unitOfWork.Folders.GetChildrenAsync(rootFolder.Id, cancellationToken);
        var specialFolder = children.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (specialFolder != null)
        {
            return specialFolder.Id;
        }

        // Erstelle den speziellen Ordner
        var newFolder = Folder.Create(name, rootFolder.Id);
        await _unitOfWork.Folders.AddAsync(newFolder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newFolder.Id;
    }

    /// <summary>
    /// Lädt den kompletten Ordnerbaum (Root-Ordner mit allen Children rekursiv).
    /// </summary>
    public async Task<List<Folder>> GetFolderTreeAsync(CancellationToken cancellationToken = default)
    {
        // Verwende GetRootWithChildrenAsync, das den Baum korrekt aufbaut
        var rootFolders = await _unitOfWork.Folders.GetRootWithChildrenAsync(cancellationToken);
        return rootFolders.ToList();
    }

    /// <summary>
    /// Erstellt einen neuen Unterordner.
    /// </summary>
    /// <param name="parentFolderId">ID des übergeordneten Ordners.</param>
    /// <param name="name">Name des neuen Ordners.</param>
    /// <returns>Der neu erstellte Ordner.</returns>
    /// <exception cref="KeyNotFoundException">Wenn der Parent-Ordner nicht existiert.</exception>
    /// <exception cref="InvalidOperationException">Wenn bereits ein Child mit gleichem Namen existiert.</exception>
    public async Task<Folder> CreateFolderAsync(Guid parentFolderId, string name, CancellationToken cancellationToken = default)
    {
        // Lade ParentFolder
        var parentFolder = await _unitOfWork.Folders.GetByIdAsync(parentFolderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Der übergeordnete Ordner mit ID {parentFolderId} wurde nicht gefunden.");

        // Prüfe, ob bereits ein Child mit gleichem Namen existiert
        if (await _unitOfWork.Folders.ExistsWithNameAsync(name, parentFolderId, cancellationToken))
        {
            throw new InvalidOperationException($"Ein Ordner mit dem Namen '{name}' existiert bereits in diesem Verzeichnis.");
        }

        // Erstelle neuen Folder
        var newFolder = Folder.Create(name, parentFolderId);

        // Speichere über Repository
        await _unitOfWork.Folders.AddAsync(newFolder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newFolder;
    }

    private async Task LoadChildrenRecursivelyAsync(Folder folder, CancellationToken cancellationToken)
    {
        var children = await _unitOfWork.Folders.GetChildrenAsync(folder.Id, cancellationToken);
        
        foreach (var child in children)
        {
            folder.Children.Add(child);
            await LoadChildrenRecursivelyAsync(child, cancellationToken);
        }
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

    public async Task<IEnumerable<FolderTreeDto>> GetFolderTreeDtosAsync(CancellationToken cancellationToken = default)
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
