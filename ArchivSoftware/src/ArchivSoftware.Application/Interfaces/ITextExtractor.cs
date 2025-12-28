namespace ArchivSoftware.Application.Interfaces;

/// <summary>
/// Interface für die Textextraktion aus verschiedenen Dateiformaten.
/// </summary>
public interface ITextExtractor
{
    /// <summary>
    /// Extrahiert Text aus einer Datei.
    /// </summary>
    /// <param name="filePath">Pfad zur Datei.</param>
    /// <returns>Extrahierter Text oder leerer String wenn nicht unterstützt.</returns>
    Task<string> ExtractAsync(string filePath);
    
    /// <summary>
    /// Prüft ob das Dateiformat unterstützt wird.
    /// </summary>
    /// <param name="filePath">Pfad zur Datei.</param>
    /// <returns>True wenn Textextraktion möglich ist.</returns>
    bool SupportsExtraction(string filePath);
}
