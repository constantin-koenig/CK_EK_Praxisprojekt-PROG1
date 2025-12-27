using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Domain.Interfaces;

/// <summary>
/// Repository-Interface für Tags.
/// </summary>
public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
