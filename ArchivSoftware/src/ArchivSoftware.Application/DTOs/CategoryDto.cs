namespace ArchivSoftware.Application.DTOs;

/// <summary>
/// DTO für Kategorieinformationen.
/// </summary>
public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int DocumentCount,
    DateTime CreatedAt);

/// <summary>
/// DTO für das Erstellen einer Kategorie.
/// </summary>
public record CreateCategoryDto(
    string Name,
    string? Description,
    Guid? ParentCategoryId);

/// <summary>
/// DTO für das Aktualisieren einer Kategorie.
/// </summary>
public record UpdateCategoryDto(
    string Name,
    string? Description,
    Guid? ParentCategoryId);
