namespace ArchivSoftware.Domain.Entities;

/// <summary>
/// Repräsentiert eine Kategorie für die Dokumentenorganisation.
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
