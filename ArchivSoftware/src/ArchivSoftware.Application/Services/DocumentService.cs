using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Domain.Entities;
using ArchivSoftware.Domain.Interfaces;

namespace ArchivSoftware.Application.Services;

/// <summary>
/// Service-Implementierung für Dokumentenoperationen.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id, cancellationToken);
        return document is null ? null : MapToDto(document);
    }

    public async Task<IEnumerable<DocumentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.GetAllAsync(cancellationToken);
        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<DocumentDto>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.GetByCategoryIdAsync(categoryId, cancellationToken);
        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<DocumentDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.SearchByTitleAsync(searchTerm, cancellationToken);
        return documents.Select(MapToDto);
    }

    public async Task<DocumentDto> CreateAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            FilePath = dto.FilePath,
            FileType = dto.FileType,
            FileSize = dto.FileSize,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Tags?.Any() == true)
        {
            foreach (var tagName in dto.Tags)
            {
                var tag = await _unitOfWork.Tags.GetByNameAsync(tagName, cancellationToken);
                if (tag is null)
                {
                    tag = new Tag
                    {
                        Id = Guid.NewGuid(),
                        Name = tagName,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Tags.AddAsync(tag, cancellationToken);
                }
                document.Tags.Add(tag);
            }
        }

        await _unitOfWork.Documents.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(document);
    }

    public async Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Document with id {id} not found.");

        document.Title = dto.Title;
        document.Description = dto.Description;
        document.CategoryId = dto.CategoryId;
        document.UpdatedAt = DateTime.UtcNow;

        if (dto.Tags is not null)
        {
            document.Tags.Clear();
            foreach (var tagName in dto.Tags)
            {
                var tag = await _unitOfWork.Tags.GetByNameAsync(tagName, cancellationToken);
                if (tag is null)
                {
                    tag = new Tag
                    {
                        Id = Guid.NewGuid(),
                        Name = tagName,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Tags.AddAsync(tag, cancellationToken);
                }
                document.Tags.Add(tag);
            }
        }

        await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(document);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Documents.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto(
            document.Id,
            document.Title,
            document.Description,
            document.FilePath,
            document.FileType,
            document.FileSize,
            document.CategoryId,
            document.Category?.Name,
            document.Tags.Select(t => t.Name),
            document.CreatedAt,
            document.UpdatedAt);
    }
}
