using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;

namespace ArchivSoftware.Application.Services;

/// <summary>
/// Service-Implementierung für Ordneroperationen.
/// Nutzt IUnitOfWorkFactory für Mandanten-Support.
/// </summary>
public class FolderService : IFolderService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public FolderService(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    /// <summary>
    /// Stellt sicher, dass ein Root-Ordner existiert. Legt "Root" an, wenn keiner existiert.
    /// </summary>
    public async Task EnsureRootFolderExistsAsync(CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        var rootFolders = await unitOfWork.Folders.GetRootFoldersAsync(cancellationToken);
        
        if (!rootFolders.Any())
        {
            var rootFolder = Folder.Create("Root", null);
            await unitOfWork.Folders.AddAsync(rootFolder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
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
        using var unitOfWork = _unitOfWorkFactory.Create();
        
        // Stelle sicher, dass Root existiert
        var rootFolders = await unitOfWork.Folders.GetRootFoldersAsync(cancellationToken);
        if (!rootFolders.Any())
        {
            var rootFolder = Folder.Create("Root", null);
            await unitOfWork.Folders.AddAsync(rootFolder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            rootFolders = await unitOfWork.Folders.GetRootFoldersAsync(cancellationToken);
        }
        
        var root = rootFolders.First();

        // Prüfe, ob der spezielle Ordner bereits existiert
        var children = await unitOfWork.Folders.GetChildrenAsync(root.Id, cancellationToken);
        var specialFolder = children.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (specialFolder != null)
        {
            return specialFolder.Id;
        }

        // Erstelle den speziellen Ordner
        var newFolder = Folder.Create(name, root.Id);
        await unitOfWork.Folders.AddAsync(newFolder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return newFolder.Id;
    }

    /// <summary>
    /// Lädt den kompletten Ordnerbaum (Root-Ordner mit allen Children rekursiv).
    /// </summary>
    public async Task<List<Folder>> GetFolderTreeAsync(CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        var rootFolders = await unitOfWork.Folders.GetRootWithChildrenAsync(cancellationToken);
        return rootFolders.ToList();
    }

    /// <summary>
    /// Erstellt einen neuen Unterordner.
    /// </summary>
    public async Task<Folder> CreateFolderAsync(Guid parentFolderId, string name, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        
        var parentFolder = await unitOfWork.Folders.GetByIdAsync(parentFolderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Der übergeordnete Ordner mit ID {parentFolderId} wurde nicht gefunden.");

        if (await unitOfWork.Folders.ExistsWithNameAsync(name, parentFolderId, cancellationToken))
        {
            throw new InvalidOperationException($"Ein Ordner mit dem Namen '{name}' existiert bereits in diesem Verzeichnis.");
        }

        var newFolder = Folder.Create(name, parentFolderId);
        await unitOfWork.Folders.AddAsync(newFolder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return newFolder;
    }

    public async Task<FolderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        var folder = await unitOfWork.Folders.GetByIdAsync(id, cancellationToken);
        return folder is null ? null : MapToDto(folder);
    }

    public async Task<IEnumerable<FolderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        var folders = await unitOfWork.Folders.GetAllAsync(cancellationToken);
        return folders.Select(MapToDto);
    }

    public async Task<IEnumerable<FolderDto>> GetRootFoldersAsync(CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        var folders = await unitOfWork.Folders.GetRootFoldersAsync(cancellationToken);
        return folders.Select(MapToDto);
    }

    public async Task<IEnumerable<FolderDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        var folders = await unitOfWork.Folders.GetChildrenAsync(parentId, cancellationToken);
        return folders.Select(MapToDto);
    }

    public async Task<IEnumerable<FolderTreeDto>> GetFolderTreeDtosAsync(CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        var rootFolders = await unitOfWork.Folders.GetRootFoldersAsync(cancellationToken);
        var treeDtos = new List<FolderTreeDto>();

        foreach (var folder in rootFolders)
        {
            treeDtos.Add(await BuildTreeDtoAsync(unitOfWork, folder, cancellationToken));
        }

        return treeDtos;
    }

    private async Task<FolderTreeDto> BuildTreeDtoAsync(IUnitOfWork unitOfWork, Folder folder, CancellationToken cancellationToken)
    {
        var children = await unitOfWork.Folders.GetChildrenAsync(folder.Id, cancellationToken);
        var childDtos = new List<FolderTreeDto>();

        foreach (var child in children)
        {
            childDtos.Add(await BuildTreeDtoAsync(unitOfWork, child, cancellationToken));
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
        using var unitOfWork = _unitOfWorkFactory.Create();
        
        if (await unitOfWork.Folders.ExistsWithNameAsync(dto.Name, dto.ParentFolderId, cancellationToken))
        {
            throw new InvalidOperationException($"Ein Ordner mit dem Namen '{dto.Name}' existiert bereits in diesem Verzeichnis.");
        }

        var folder = Folder.Create(dto.Name, dto.ParentFolderId);
        await unitOfWork.Folders.AddAsync(folder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(folder);
    }

    public async Task<FolderDto> UpdateAsync(Guid id, UpdateFolderDto dto, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        
        var folder = await unitOfWork.Folders.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {id} wurde nicht gefunden.");

        if (folder.Name != dto.Name && 
            await unitOfWork.Folders.ExistsWithNameAsync(dto.Name, folder.ParentFolderId, cancellationToken))
        {
            throw new InvalidOperationException($"Ein Ordner mit dem Namen '{dto.Name}' existiert bereits in diesem Verzeichnis.");
        }

        folder.Name = dto.Name;
        folder.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.Folders.UpdateAsync(folder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(folder);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        
        var folder = await unitOfWork.Folders.GetWithChildrenAndDocumentsAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {id} wurde nicht gefunden.");

        if (folder.Children.Any())
        {
            throw new InvalidOperationException("Der Ordner kann nicht gelöscht werden, da er Unterordner enthält.");
        }

        if (folder.Documents.Any())
        {
            throw new InvalidOperationException("Der Ordner kann nicht gelöscht werden, da er Dokumente enthält.");
        }

        await unitOfWork.Folders.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<FolderDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        var folders = await unitOfWork.Folders.SearchByNameAsync(searchTerm, cancellationToken);
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

    /// <summary>
    /// Benennt einen Ordner um.
    /// </summary>
    public async Task RenameFolderAsync(Guid folderId, string newName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Der Ordnername darf nicht leer sein.", nameof(newName));

        using var unitOfWork = _unitOfWorkFactory.Create();
        
        var folder = await unitOfWork.Folders.GetByIdAsync(folderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {folderId} wurde nicht gefunden.");

        if (folder.ParentFolderId == null)
            throw new InvalidOperationException("Der Root-Ordner kann nicht umbenannt werden.");

        if (!folder.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) &&
            await unitOfWork.Folders.ExistsWithNameAsync(newName, folder.ParentFolderId, cancellationToken))
        {
            throw new InvalidOperationException($"Ein Ordner mit dem Namen '{newName}' existiert bereits in diesem Verzeichnis.");
        }

        folder.Rename(newName);
        await unitOfWork.Folders.UpdateAsync(folder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Löscht einen Ordner inklusive aller Unterordner und Dokumente (Cascade).
    /// </summary>
    public async Task DeleteFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkFactory.Create();
        
        var folder = await unitOfWork.Folders.GetByIdAsync(folderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {folderId} wurde nicht gefunden.");

        if (folder.ParentFolderId == null)
            throw new InvalidOperationException("Der Root-Ordner kann nicht gelöscht werden.");

        await DeleteFolderRecursiveAsync(unitOfWork, folderId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteFolderRecursiveAsync(IUnitOfWork unitOfWork, Guid folderId, CancellationToken cancellationToken)
    {
        var children = await unitOfWork.Folders.GetChildrenAsync(folderId, cancellationToken);
        
        foreach (var child in children)
        {
            await DeleteFolderRecursiveAsync(unitOfWork, child.Id, cancellationToken);
        }

        var documents = await unitOfWork.Documents.GetByFolderIdAsync(folderId, cancellationToken);
        foreach (var doc in documents)
        {
            await unitOfWork.Documents.DeleteAsync(doc.Id, cancellationToken);
        }

        await unitOfWork.Folders.DeleteAsync(folderId, cancellationToken);
    }

    /// <summary>
    /// Verschiebt einen Ordner zu einem neuen Parent.
    /// </summary>
    public async Task MoveFolderAsync(Guid folderId, Guid newParentId, CancellationToken cancellationToken = default)
    {
        if (folderId == newParentId)
            throw new InvalidOperationException("Ein Ordner kann nicht in sich selbst verschoben werden.");

        using var unitOfWork = _unitOfWorkFactory.Create();
        
        var folder = await unitOfWork.Folders.GetByIdAsync(folderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {folderId} wurde nicht gefunden.");

        var newParent = await unitOfWork.Folders.GetByIdAsync(newParentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Zielordner mit ID {newParentId} wurde nicht gefunden.");

        if (folder.ParentFolderId == null)
            throw new InvalidOperationException("Der Root-Ordner kann nicht verschoben werden.");

        if (await IsDescendantOfAsync(unitOfWork, newParentId, folderId, cancellationToken))
            throw new InvalidOperationException("Ein Ordner kann nicht in einen seiner Unterordner verschoben werden.");

        if (await unitOfWork.Folders.ExistsWithNameAsync(folder.Name, newParentId, cancellationToken))
            throw new InvalidOperationException($"Im Zielordner existiert bereits ein Ordner mit dem Namen '{folder.Name}'.");

        folder.ParentFolderId = newParentId;
        folder.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.Folders.UpdateAsync(folder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> IsDescendantOfAsync(IUnitOfWork unitOfWork, Guid potentialDescendantId, Guid ancestorId, CancellationToken cancellationToken)
    {
        var current = await unitOfWork.Folders.GetByIdAsync(potentialDescendantId, cancellationToken);
        
        while (current != null)
        {
            if (current.Id == ancestorId)
                return true;
                
            if (current.ParentFolderId == null)
                break;
                
            current = await unitOfWork.Folders.GetByIdAsync(current.ParentFolderId.Value, cancellationToken);
        }
        
        return false;
    }
}
