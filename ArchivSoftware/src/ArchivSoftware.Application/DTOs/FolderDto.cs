namespace ArchivSoftware.Application.DTOs;

/// <summary>
/// DTO für Ordnerinformationen.
/// </summary>
public record FolderDto(
    Guid Id,
    string Name,
    Guid? ParentFolderId,
    int ChildCount,
    int DocumentCount,
    DateTime CreatedAt);

/// <summary>
/// DTO für das Erstellen eines Ordners.
/// </summary>
public record CreateFolderDto(
    string Name,
    Guid? ParentFolderId);

/// <summary>
/// DTO für das Aktualisieren eines Ordners.
/// </summary>
public record UpdateFolderDto(
    string Name);

/// <summary>
/// DTO für einen Ordner mit seinen Kindern (Tree-Ansicht).
/// </summary>
public record FolderTreeDto(
    Guid Id,
    string Name,
    Guid? ParentFolderId,
    IEnumerable<FolderTreeDto> Children,
    int DocumentCount,
    DateTime CreatedAt);
