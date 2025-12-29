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

    /// <summary>
    /// Benennt einen Ordner um. Validiert Namen und prüft Uniqueness pro Parent.
    /// </summary>
    public async Task RenameFolderAsync(Guid folderId, string newName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Der Ordnername darf nicht leer sein.", nameof(newName));

        var folder = await _unitOfWork.Folders.GetByIdAsync(folderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {folderId} wurde nicht gefunden.");

        // Root-Ordner darf nicht umbenannt werden
        if (folder.ParentFolderId == null)
            throw new InvalidOperationException("Der Root-Ordner kann nicht umbenannt werden.");

        // Prüfe Uniqueness im Parent
        if (!folder.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) &&
            await _unitOfWork.Folders.ExistsWithNameAsync(newName, folder.ParentFolderId, cancellationToken))
        {
            throw new InvalidOperationException($"Ein Ordner mit dem Namen '{newName}' existiert bereits in diesem Verzeichnis.");
        }

        folder.Rename(newName);
        await _unitOfWork.Folders.UpdateAsync(folder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Löscht einen Ordner inklusive aller Unterordner und Dokumente (Cascade).
    /// </summary>
    public async Task DeleteFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        var folder = await _unitOfWork.Folders.GetByIdAsync(folderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {folderId} wurde nicht gefunden.");

        // Root-Ordner darf nicht gelöscht werden
        if (folder.ParentFolderId == null)
            throw new InvalidOperationException("Der Root-Ordner kann nicht gelöscht werden.");

        // Rekursiv löschen (Children zuerst)
        await DeleteFolderRecursiveAsync(folderId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteFolderRecursiveAsync(Guid folderId, CancellationToken cancellationToken)
    {
        // Hole alle Unterordner
        var children = await _unitOfWork.Folders.GetChildrenAsync(folderId, cancellationToken);
        
        // Rekursiv Unterordner löschen
        foreach (var child in children)
        {
            await DeleteFolderRecursiveAsync(child.Id, cancellationToken);
        }

        // Dokumente des Ordners löschen
        var documents = await _unitOfWork.Documents.GetByFolderIdAsync(folderId, cancellationToken);
        foreach (var doc in documents)
        {
            await _unitOfWork.Documents.DeleteAsync(doc.Id, cancellationToken);
        }

        // Ordner selbst löschen
        await _unitOfWork.Folders.DeleteAsync(folderId, cancellationToken);
    }

    /// <summary>
    /// Verschiebt einen Ordner zu einem neuen Parent. Prüft auf Zyklen und Uniqueness.
    /// </summary>
    public async Task MoveFolderAsync(Guid folderId, Guid newParentId, CancellationToken cancellationToken = default)
    {
        if (folderId == newParentId)
            throw new InvalidOperationException("Ein Ordner kann nicht in sich selbst verschoben werden.");

        var folder = await _unitOfWork.Folders.GetByIdAsync(folderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ordner mit ID {folderId} wurde nicht gefunden.");

        var newParent = await _unitOfWork.Folders.GetByIdAsync(newParentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Zielordner mit ID {newParentId} wurde nicht gefunden.");

        // Root-Ordner darf nicht verschoben werden
        if (folder.ParentFolderId == null)
            throw new InvalidOperationException("Der Root-Ordner kann nicht verschoben werden.");

        // Prüfe auf Zyklen: newParent darf kein Nachfahre von folder sein
        if (await IsDescendantOfAsync(newParentId, folderId, cancellationToken))
            throw new InvalidOperationException("Ein Ordner kann nicht in einen seiner Unterordner verschoben werden.");

        // Prüfe Uniqueness im Zielordner
        if (await _unitOfWork.Folders.ExistsWithNameAsync(folder.Name, newParentId, cancellationToken))
            throw new InvalidOperationException($"Im Zielordner existiert bereits ein Ordner mit dem Namen '{folder.Name}'.");

        // Verschieben
        folder.ParentFolderId = newParentId;
        folder.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Folders.UpdateAsync(folder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Prüft ob potentialDescendantId ein Nachfahre von ancestorId ist.
    /// </summary>
    private async Task<bool> IsDescendantOfAsync(Guid potentialDescendantId, Guid ancestorId, CancellationToken cancellationToken)
    {
        var current = await _unitOfWork.Folders.GetByIdAsync(potentialDescendantId, cancellationToken);
        
        while (current != null)
        {
            if (current.Id == ancestorId)
                return true;
                
            if (current.ParentFolderId == null)
                break;
                
            current = await _unitOfWork.Folders.GetByIdAsync(current.ParentFolderId.Value, cancellationToken);
        }
        
        return false;
    }
}
