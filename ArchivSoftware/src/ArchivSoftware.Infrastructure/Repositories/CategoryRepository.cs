using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure.Repositories;

/// <summary>
/// Repository-Implementierung für Kategorien.
/// </summary>
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ArchivDbContext context) : base(context)
    {
    }

    public override async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Documents)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Documents)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Documents)
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Documents)
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == parentId)
            .ToListAsync(cancellationToken);
    }
}
