namespace ArchivSoftware.Domain.Entities;

/// <summary>
/// Repräsentiert einen Tag zur Verschlagwortung von Dokumenten.
/// </summary>
public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
