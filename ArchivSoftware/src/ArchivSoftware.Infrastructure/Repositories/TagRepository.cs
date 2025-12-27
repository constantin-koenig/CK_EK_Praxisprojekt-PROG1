using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchivSoftware.Infrastructure.Repositories;

/// <summary>
/// Repository-Implementierung für Tags.
/// </summary>
public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(ArchivDbContext context) : base(context)
    {
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }
}
