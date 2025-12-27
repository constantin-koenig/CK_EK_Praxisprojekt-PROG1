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
    public DocumentRepository(ArchivDbContext context) : base(context)
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
            .Include(d => d.Folder)
            .Where(d => d.FolderId == folderId)
            .OrderBy(d => d.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Folder)
            .Where(d => d.Title.Contains(searchTerm))
            .OrderBy(d => d.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> SearchByContentAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Folder)
            .Where(d => d.PlainText.Contains(searchTerm))
            .OrderBy(d => d.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Folder)
            .Where(d => d.Title.Contains(searchTerm) || d.PlainText.Contains(searchTerm) || d.FileName.Contains(searchTerm))
            .OrderBy(d => d.Title)
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
