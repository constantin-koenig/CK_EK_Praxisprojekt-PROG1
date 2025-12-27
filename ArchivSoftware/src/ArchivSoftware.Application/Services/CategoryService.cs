using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;

namespace ArchivSoftware.Application.Services;

/// <summary>
/// Service-Implementierung für Kategorieoperationen.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        return category is null ? null : MapToDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        return categories.Select(MapToDto);
    }

    public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetRootCategoriesAsync(cancellationToken);
        return categories.Select(MapToDto);
    }

    public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetSubCategoriesAsync(parentId, cancellationToken);
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {id} not found.");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Categories.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ParentCategoryId,
            category.Documents.Count,
            category.CreatedAt);
    }
}
