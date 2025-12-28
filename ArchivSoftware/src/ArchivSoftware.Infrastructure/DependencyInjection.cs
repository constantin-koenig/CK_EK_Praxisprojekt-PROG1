using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Application.Services;
using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArchivSoftware.Infrastructure;

/// <summary>
/// Extension-Methoden für die Dependency Injection Konfiguration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registriert die Infrastructure-Services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ArchivDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Registriert die Application-Services.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IFolderService, FolderService>();
        services.AddScoped<ISearchService, SearchService>();

        return services;
    }
}
