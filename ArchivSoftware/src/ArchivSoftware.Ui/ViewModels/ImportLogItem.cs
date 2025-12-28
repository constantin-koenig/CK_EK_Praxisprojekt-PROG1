using ArchivSoftware.Application.Interfaces;

namespace ArchivSoftware.Ui.ViewModels;

/// <summary>
/// Repräsentiert einen Eintrag im Import-Log.
/// </summary>
public class ImportLogItem
{
    public DateTime Timestamp { get; init; }
    public string FileName { get; init; } = string.Empty;
    public ImportStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gibt eine formatierte Darstellung des Status zurück.
    /// </summary>
    public string StatusText => Status switch
    {
        ImportStatus.Started => "⏳ Gestartet",
        ImportStatus.Imported => "✅ Importiert",
        ImportStatus.Ignored => "⚠️ Ignoriert",
        ImportStatus.Failed => "❌ Fehlgeschlagen",
        _ => Status.ToString()
    };
}
