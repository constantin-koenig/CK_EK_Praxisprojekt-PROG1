namespace ArchivSoftware.Application.Options;

/// <summary>
/// Optionen für den automatischen Import-Überwacher.
/// </summary>
public class ImportWatcherOptions
{
    /// <summary>
    /// Gibt an, ob der Import-Überwacher aktiviert ist.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Der zu überwachende Ordnerpfad.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Der Zielordner in der Archivstruktur (Name).
    /// </summary>
    public string TargetFolder { get; set; } = string.Empty;
}
