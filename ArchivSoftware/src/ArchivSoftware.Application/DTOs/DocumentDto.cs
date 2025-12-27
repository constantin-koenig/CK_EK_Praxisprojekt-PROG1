namespace ArchivSoftware.Application.DTOs;

/// <summary>
/// DTO für Dokumenteninformationen (ohne Binärdaten).
/// </summary>
public record DocumentDto(
    Guid Id,
    string Title,
    string FileName,
    string ContentType,
    long DataSize,
    string? Sha256,
    Guid FolderId,
    string? FolderName,
    DateTime CreatedAt);

/// <summary>
/// DTO für das Erstellen eines Dokuments.
/// </summary>
public record CreateDocumentDto(
    string Title,
    string FileName,
    string ContentType,
    byte[] Data,
    Guid FolderId,
    string? PlainText);

/// <summary>
/// DTO für das Aktualisieren eines Dokuments.
/// </summary>
public record UpdateDocumentDto(
    string Title);

/// <summary>
/// DTO für Dokumentendaten (mit Binärdaten).
/// </summary>
public record DocumentDataDto(
    Guid Id,
    string Title,
    string FileName,
    string ContentType,
    byte[] Data,
    string PlainText,
    string? Sha256,
    Guid FolderId,
    DateTime CreatedAt);
