namespace ArchivSoftware.Domain.Entities;

/// <summary>
/// Repräsentiert einen Ordner in der Archivstruktur.
/// </summary>
public class Folder : BaseEntity
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            ValidateName(value);
            _name = value;
        }
    }

    public Guid? ParentFolderId { get; set; }
    public Folder? ParentFolder { get; set; }
    public ICollection<Folder> Children { get; set; } = new List<Folder>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();

    /// <summary>
    /// Erstellt einen neuen Ordner.
    /// </summary>
    public static Folder Create(string name, Guid? parentFolderId = null)
    {
        ValidateName(name);
        
        return new Folder
        {
            Id = Guid.NewGuid(),
            Name = name,
            ParentFolderId = parentFolderId,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Der Ordnername darf nicht leer sein.", nameof(name));
        }

        if (name.Length > 255)
        {
            throw new ArgumentException("Der Ordnername darf maximal 255 Zeichen lang sein.", nameof(name));
        }

        // Prüfe auf ungültige Zeichen im Dateinamen
        var invalidChars = Path.GetInvalidFileNameChars();
        if (name.IndexOfAny(invalidChars) >= 0)
        {
            throw new ArgumentException("Der Ordnername enthält ungültige Zeichen.", nameof(name));
        }
    }
}
