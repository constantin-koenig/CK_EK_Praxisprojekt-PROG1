namespace ArchivSoftware.Application;

/// <summary>
/// Zentrale Policy für erlaubte Dateitypen.
/// Definiert welche Dateiformate importiert werden dürfen.
/// </summary>
public static class FileTypePolicy
{
    /// <summary>
    /// Liste der erlaubten Dateiendungen (lowercase, mit Punkt).
    /// </summary>
    public static readonly string[] AllowedExtensions = { ".pdf", ".docx" };

    /// <summary>
    /// Filter-String für OpenFileDialog.
    /// </summary>
    public const string OpenFileDialogFilter = "PDF/DOCX Dateien (*.pdf;*.docx)|*.pdf;*.docx";

    /// <summary>
    /// Prüft ob eine Datei anhand ihrer Extension erlaubt ist.
    /// </summary>
    /// <param name="filePath">Pfad oder Dateiname mit Extension.</param>
    /// <returns>True wenn der Dateityp erlaubt ist.</returns>
    public static bool IsAllowed(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var extension = Path.GetExtension(filePath);
        return AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gibt den MIME Content-Type für eine Datei zurück.
    /// </summary>
    /// <param name="filePath">Pfad oder Dateiname mit Extension.</param>
    /// <returns>MIME Content-Type.</returns>
    public static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Gibt eine benutzerfreundliche Fehlermeldung zurück.
    /// </summary>
    public const string NotAllowedMessage = "Nur PDF und DOCX Dateien sind erlaubt.";
}
