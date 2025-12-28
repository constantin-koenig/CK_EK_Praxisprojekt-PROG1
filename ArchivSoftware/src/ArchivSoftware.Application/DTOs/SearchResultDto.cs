namespace ArchivSoftware.Application.DTOs;

/// <summary>
/// DTO für Suchergebnisse mit Snippet.
/// </summary>
public record SearchResultDto(
    Guid DocumentId,
    string Title,
    string FileName,
    Guid FolderId,
    string FolderPath,
    string Snippet);
