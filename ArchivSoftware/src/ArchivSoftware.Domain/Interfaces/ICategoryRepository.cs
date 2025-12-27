using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Domain.Interfaces;

/// <summary>
/// Repository-Interface für Kategorien.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
}
