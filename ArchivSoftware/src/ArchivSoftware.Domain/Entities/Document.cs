namespace ArchivSoftware.Domain.Entities;

/// <summary>
/// Repräsentiert ein archiviertes Dokument.
/// </summary>
public class Document : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
