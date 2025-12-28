namespace ArchivSoftware.Application.Interfaces;

/// <summary>
/// Status eines Import-Vorgangs.
/// </summary>
public enum ImportStatus
{
    Started,
    Imported,
    Ignored,
    Failed
}

/// <summary>
/// Schnittstelle für das Logging von Import-Vorgängen.
/// </summary>
public interface IImportLogSink
{
    /// <summary>
    /// Fügt einen Log-Eintrag hinzu.
    /// </summary>
    /// <param name="fileName">Name der Datei.</param>
    /// <param name="status">Status des Imports.</param>
    /// <param name="message">Zusätzliche Nachricht.</param>
    void Add(string fileName, ImportStatus status, string message);
}
