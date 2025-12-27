using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure.Repositories;

/// <summary>
/// Repository-Implementierung für Ordner.
/// </summary>
public class FolderRepository : Repository<Folder>, IFolderRepository
{
    public FolderRepository(ArchivDbContext context) : base(context)
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
            .Include(f => f.Children)
            .Include(f => f.Documents)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Folder>> GetRootFoldersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Children)
            .Include(f => f.Documents)
            .Where(f => f.ParentFolderId == null)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Folder>> GetChildrenAsync(Guid parentFolderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
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
            .Include(f => f.Documents)
            .Where(f => f.Name.Contains(searchTerm))
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, Guid? parentFolderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(f => f.Name == name && f.ParentFolderId == parentFolderId, cancellationToken);
    }
}
