using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ArchivSoftware.Infrastructure.Services;

/// <summary>
/// Factory für ArchivSoftwareDbContext mit dynamischem ConnectionString basierend auf dem aktuellen Mandanten.
/// </summary>
public class TenantDbContextFactory : IDbContextFactory<ArchivSoftwareDbContext>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IConfiguration _configuration;

    public TenantDbContextFactory(ITenantProvider tenantProvider, IConfiguration configuration)
    {
        _tenantProvider = tenantProvider;
        _configuration = configuration;
    }

    public ArchivSoftwareDbContext CreateDbContext()
    {
        var connectionString = _configuration.GetConnectionString(_tenantProvider.CurrentTenant)
            ?? throw new InvalidOperationException($"ConnectionString für Mandant '{_tenantProvider.CurrentTenant}' nicht gefunden.");

        var optionsBuilder = new DbContextOptionsBuilder<ArchivSoftwareDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ArchivSoftwareDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Stellt sicher, dass die Datenbank für den aktuellen Mandanten existiert und initialisiert ist.
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync()
    {
        using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Führt Migrationen für den aktuellen Mandanten aus.
    /// </summary>
    public async Task MigrateDatabaseAsync()
    {
        using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }
}
