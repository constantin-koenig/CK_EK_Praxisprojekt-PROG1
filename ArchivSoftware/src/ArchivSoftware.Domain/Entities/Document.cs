namespace ArchivSoftware.Domain.Entities;

/// <summary>
/// Repräsentiert ein archiviertes Dokument.
/// </summary>
public class Document : BaseEntity
{
    private string _title = string.Empty;
    private string _fileName = string.Empty;
    private string _contentType = string.Empty;
    private byte[] _data = Array.Empty<byte>();

    public string Title
    {
        get => _title;
        set
        {
            ValidateTitle(value);
            _title = value;
        }
    }

    public string FileName
    {
        get => _fileName;
        set
        {
            ValidateFileName(value);
            _fileName = value;
        }
    }

    public string ContentType
    {
        get => _contentType;
        set
        {
            ValidateContentType(value);
            _contentType = value;
        }
    }

    public byte[] Data
    {
        get => _data;
        set
        {
            ValidateData(value);
            _data = value;
        }
    }

    public string PlainText { get; set; } = string.Empty;
    public string? Sha256 { get; set; }
    public Guid FolderId { get; set; }
    public Folder? Folder { get; set; }

    /// <summary>
    /// Erstellt ein neues Dokument.
    /// </summary>
    public static Document Create(string title, string fileName, string contentType, byte[] data, Guid folderId, string? plainText = null)
    {
        ValidateTitle(title);
        ValidateFileName(fileName);
        ValidateContentType(contentType);
        ValidateData(data);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            _title = title,
            _fileName = fileName,
            _contentType = contentType,
            _data = data,
            PlainText = plainText ?? string.Empty,
            FolderId = folderId,
            CreatedAt = DateTime.UtcNow
        };

        document.Sha256 = ComputeSha256(data);
        return document;
    }

    /// <summary>
    /// Berechnet den SHA256-Hash der Daten.
    /// </summary>
    public void ComputeHash()
    {
        Sha256 = ComputeSha256(Data);
    }

    private static string ComputeSha256(byte[] data)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(data);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Der Dokumenttitel darf nicht leer sein.", nameof(title));
        }

        if (title.Length > 500)
        {
            throw new ArgumentException("Der Dokumenttitel darf maximal 500 Zeichen lang sein.", nameof(title));
        }
    }

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Der Dateiname darf nicht leer sein.", nameof(fileName));
        }

        if (fileName.Length > 255)
        {
            throw new ArgumentException("Der Dateiname darf maximal 255 Zeichen lang sein.", nameof(fileName));
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidChars) >= 0)
        {
            throw new ArgumentException("Der Dateiname enthält ungültige Zeichen.", nameof(fileName));
        }
    }

    private static void ValidateContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Der Content-Type darf nicht leer sein.", nameof(contentType));
        }
    }

    private static void ValidateData(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            throw new ArgumentException("Die Dokumentdaten dürfen nicht leer sein.", nameof(data));
        }
    }
}
