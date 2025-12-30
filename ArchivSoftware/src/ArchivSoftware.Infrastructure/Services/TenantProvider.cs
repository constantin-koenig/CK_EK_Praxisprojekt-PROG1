using ArchivSoftware.Application.Interfaces;

namespace ArchivSoftware.Infrastructure.Services;

/// <summary>
/// Implementierung des Mandanten-Providers.
/// </summary>
public class TenantProvider : ITenantProvider
{
    private string _currentTenant;
    private readonly List<string> _availableTenants;

    public TenantProvider(IEnumerable<string> availableTenants, string defaultTenant = "TenantA")
    {
        _availableTenants = availableTenants.ToList();
        _currentTenant = defaultTenant;
    }

    public string CurrentTenant
    {
        get => _currentTenant;
        set
        {
            if (_currentTenant != value && _availableTenants.Contains(value))
            {
                _currentTenant = value;
                TenantChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<string>? TenantChanged;

    public IReadOnlyList<string> AvailableTenants => _availableTenants.AsReadOnly();
}
