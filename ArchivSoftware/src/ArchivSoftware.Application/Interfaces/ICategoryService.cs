using ArchivSoftware.Application.DTOs;

namespace ArchivSoftware.Application.Interfaces;

/// <summary>
/// Service-Interface für Kategorieoperationen.
/// </summary>
public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
