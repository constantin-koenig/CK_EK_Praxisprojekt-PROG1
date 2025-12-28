using ArchivSoftware.Application.DTOs;

namespace ArchivSoftware.Application.Interfaces;

/// <summary>
/// Service-Interface für die Dokumentensuche.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Sucht Dokumente nach Titel, Inhalt oder Dateiname.
    /// </summary>
    /// <param name="term">Suchbegriff.</param>
    /// <param name="cancellationToken">Abbruchtoken.</param>
    /// <returns>Liste von Suchergebnissen mit Snippets.</returns>
    Task<List<SearchResultDto>> SearchAsync(string term, CancellationToken cancellationToken = default);
}
