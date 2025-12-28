using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure.Repositories;

/// <summary>
/// Repository-Implementierung für Dokumente.
/// </summary>
public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ArchivSoftwareDbContext context) : base(context)
    {
    }

    public override async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Folder)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Folder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(d => d.Folder)
            .Where(d => d.FolderId == folderId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var pattern = $"%{searchTerm.Trim()}%";
        
        return await _dbSet
            .AsNoTracking()
            .Include(d => d.Folder)
            .Where(d => EF.Functions.Like(d.Title, pattern))
            .OrderByDescending(d => d.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> SearchByContentAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var pattern = $"%{searchTerm.Trim()}%";
        
        return await _dbSet
            .AsNoTracking()
            .Include(d => d.Folder)
            .Where(d => EF.Functions.Like(d.PlainText, pattern))
            .OrderByDescending(d => d.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var pattern = $"%{searchTerm.Trim()}%";
        
        return await _dbSet
            .AsNoTracking()
            .Include(d => d.Folder)
            .Where(d => EF.Functions.Like(d.Title, pattern) 
                     || EF.Functions.Like(d.PlainText, pattern) 
                     || EF.Functions.Like(d.FileName, pattern))
            .OrderByDescending(d => d.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document?> GetBySha256Async(string sha256, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Folder)
            .FirstOrDefaultAsync(d => d.Sha256 == sha256, cancellationToken);
    }

    public async Task<bool> ExistsWithHashAsync(string sha256, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(d => d.Sha256 == sha256, cancellationToken);
    }

    public async Task<Document?> GetWithDataAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Folder)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
}
