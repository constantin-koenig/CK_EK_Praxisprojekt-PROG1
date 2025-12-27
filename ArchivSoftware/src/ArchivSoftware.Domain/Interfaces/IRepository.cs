using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Domain.Interfaces;

/// <summary>
/// Generisches Repository-Interface für CRUD-Operationen.
/// </summary>
/// <typeparam name="T">Entity-Typ, der von BaseEntity erbt.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
