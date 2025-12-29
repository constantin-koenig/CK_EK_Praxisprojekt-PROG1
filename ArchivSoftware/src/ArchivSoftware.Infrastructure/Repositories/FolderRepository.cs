using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure.Repositories;

/// <summary>
/// Repository-Implementierung für Ordner mit EF Core.
/// </summary>
public class FolderRepository : Repository<Folder>, IFolderRepository
{
    public FolderRepository(ArchivSoftwareDbContext context) : base(context)
    {
    }

    public override async Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Children)
            .Include(f => f.Documents)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Folder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(f => f.Children)
            .Include(f => f.Documents)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Folder>> GetRootFoldersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(f => f.Children)
            .Include(f => f.Documents)
            .Where(f => f.ParentFolderId == null)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Folder>> GetRootWithChildrenAsync(CancellationToken cancellationToken = default)
    {
        // Lade alle Ordner und baue den Baum im Speicher auf
        var allFolders = await _dbSet
            .AsNoTracking()
            .Include(f => f.Documents)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

        // Gruppiere nach ParentFolderId
        var lookup = allFolders.ToLookup(f => f.ParentFolderId);

        // Weise Children zu
        foreach (var folder in allFolders)
        {
            foreach (var child in lookup[folder.Id])
            {
                folder.Children.Add(child);
            }
        }

        // Gib nur Root-Ordner zurück
        return allFolders.Where(f => f.ParentFolderId == null);
    }

    public async Task<bool> ExistsRootAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(f => f.ParentFolderId == null, cancellationToken);
    }

    public async Task<IEnumerable<Folder>> GetChildrenAsync(Guid parentFolderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(f => f.Children)
            .Include(f => f.Documents)
            .Where(f => f.ParentFolderId == parentFolderId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Folder?> GetWithChildrenAndDocumentsAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Children)
                .ThenInclude(c => c.Children)
            .Include(f => f.Documents)
            .FirstOrDefaultAsync(f => f.Id == folderId, cancellationToken);
    }

    public async Task<IEnumerable<Folder>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(f => f.Documents)
            .Where(f => f.Name.Contains(searchTerm))
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, Guid? parentFolderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(f => f.Name == name && f.ParentFolderId == parentFolderId, cancellationToken);
    }

    public async Task<bool> ChildFolderExistsAsync(Guid parentId, string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(f => f.ParentFolderId == parentId && f.Name == name, cancellationToken);
    }

    public async Task<Folder?> GetByIdWithParentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.ParentFolder)
            .Include(f => f.Children)
            .Include(f => f.Documents)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<bool> IsDescendantAsync(Guid potentialDescendantId, Guid ancestorId, CancellationToken cancellationToken = default)
    {
        // Iterativ die Parent-Chain durchlaufen
        var currentId = potentialDescendantId;
        
        while (true)
        {
            if (currentId == ancestorId)
                return true;

            var folder = await _dbSet
                .AsNoTracking()
                .Where(f => f.Id == currentId)
                .Select(f => new { f.ParentFolderId })
                .FirstOrDefaultAsync(cancellationToken);

            if (folder == null || folder.ParentFolderId == null)
                return false;

            currentId = folder.ParentFolderId.Value;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
