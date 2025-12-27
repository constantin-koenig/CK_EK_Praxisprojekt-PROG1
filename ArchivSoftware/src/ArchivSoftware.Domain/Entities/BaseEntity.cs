namespace ArchivSoftware.Domain.Entities;

/// <summary>
/// Basisklasse für alle Entities mit einer Id.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
