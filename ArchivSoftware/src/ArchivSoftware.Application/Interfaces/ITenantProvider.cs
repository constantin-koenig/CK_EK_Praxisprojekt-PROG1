namespace ArchivSoftware.Application.Interfaces;

/// <summary>
/// Provider für den aktuellen Mandanten.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Der aktuelle Mandant (z.B. "TenantA" oder "TenantB").
    /// </summary>
    string CurrentTenant { get; set; }

    /// <summary>
    /// Event das ausgelöst wird, wenn der Mandant gewechselt wird.
    /// </summary>
    event EventHandler<string>? TenantChanged;

    /// <summary>
    /// Liste der verfügbaren Mandanten.
    /// </summary>
    IReadOnlyList<string> AvailableTenants { get; }
}
