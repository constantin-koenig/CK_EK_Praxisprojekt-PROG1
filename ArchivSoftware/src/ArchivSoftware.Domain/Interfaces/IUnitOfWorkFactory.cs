namespace ArchivSoftware.Domain.Interfaces;

/// <summary>
/// Factory für die Erstellung von UnitOfWork-Instanzen.
/// Ermöglicht Mandanten-Wechsel zur Laufzeit.
/// </summary>
public interface IUnitOfWorkFactory
{
    /// <summary>
    /// Erstellt eine neue UnitOfWork-Instanz für den aktuellen Mandanten.
    /// </summary>
    IUnitOfWork Create();
}
