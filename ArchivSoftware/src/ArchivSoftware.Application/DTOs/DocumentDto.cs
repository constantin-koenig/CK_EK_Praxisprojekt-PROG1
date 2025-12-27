using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Application.DTOs;

/// <summary>
/// DTO für Dokumenteninformationen.
/// </summary>
public record DocumentDto(
    Guid Id,
    string Title,
    string? Description,
    string FilePath,
    string FileType,
    long FileSize,
    Guid? CategoryId,
    string? CategoryName,
    IEnumerable<string> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// DTO für das Erstellen eines Dokuments.
/// </summary>
public record CreateDocumentDto(
    string Title,
    string? Description,
    string FilePath,
    string FileType,
    long FileSize,
    Guid? CategoryId,
    IEnumerable<string>? Tags);

/// <summary>
/// DTO für das Aktualisieren eines Dokuments.
/// </summary>
public record UpdateDocumentDto(
    string Title,
    string? Description,
    Guid? CategoryId,
    IEnumerable<string>? Tags);
