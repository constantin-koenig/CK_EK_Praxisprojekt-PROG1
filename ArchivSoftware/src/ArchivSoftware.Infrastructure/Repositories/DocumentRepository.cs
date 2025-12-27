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
            .Include(d => d.Category)
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Tags)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Tags)
            .Where(d => d.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Tags)
            .Where(d => d.Title.Contains(searchTerm) || (d.Description != null && d.Description.Contains(searchTerm)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByTagAsync(string tagName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Tags)
            .Where(d => d.Tags.Any(t => t.Name == tagName))
            .ToListAsync(cancellationToken);
    }
}
