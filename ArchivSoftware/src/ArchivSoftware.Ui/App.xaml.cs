using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Application.Options;
using ArchivSoftware.Application.Services;
using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure;
using ArchivSoftware.Infrastructure.Data;
using ArchivSoftware.Infrastructure.Repositories;
using ArchivSoftware.Infrastructure.Services;
using ArchivSoftware.Ui.Services;
using ArchivSoftware.Ui.ViewModels;
using ArchivSoftware.Ui.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ArchivSoftware.Ui;

/// <summary>
/// Hauptanwendungsklasse mit Host-basierter Dependency Injection.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;
    private IServiceScope? _appScope;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context.Configuration, services);
            })
            .Build();
    }

    private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Konfiguration als Service registrieren
        services.AddSingleton<IConfiguration>(configuration);

        // Verfügbare Mandanten aus ConnectionStrings ermitteln
        var connectionStrings = configuration.GetSection("ConnectionStrings").GetChildren()
            .Select(c => c.Key)
            .ToList();

        // TenantProvider als Singleton
        var tenantProvider = new TenantProvider(connectionStrings, connectionStrings.FirstOrDefault() ?? "TenantA");
        services.AddSingleton<ITenantProvider>(tenantProvider);

        // Options
        services.Configure<ImportWatcherOptions>(configuration.GetSection("ImportWatcher"));
        
        // ImportWatcherOptions als Singleton für direkten Zugriff
        var importWatcherOptions = configuration.GetSection("ImportWatcher").Get<ImportWatcherOptions>() ?? new ImportWatcherOptions();
        services.AddSingleton(importWatcherOptions);

        // DbContextFactory mit Mandanten-Support
        services.AddSingleton<IDbContextFactory<ArchivSoftwareDbContext>, TenantDbContextFactory>();
        services.AddSingleton<TenantDbContextFactory>();

        // Unit of Work Factory - erstellt bei jeder Operation einen neuen UnitOfWork
        services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();

        // Infrastructure Services
        services.AddSingleton<ITextExtractor, TextExtractor>();
        
        // Import-Log: Singleton ObservableCollection und ImportLogSink
        var importLog = new ObservableCollection<ImportLogItem>();
        services.AddSingleton(importLog);
        services.AddSingleton<IImportLogSink>(sp => new ImportLogSink(importLog));
        
        services.AddHostedService<ImportWatcherService>();

        // Application Services - nutzen jetzt IUnitOfWorkFactory, daher als Singleton
        services.AddSingleton<IFolderService, FolderService>();
        services.AddSingleton<IDocumentService, DocumentService>();
        services.AddSingleton<ISearchService, SearchService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<DocumentListViewModel>();
        services.AddTransient<FolderTreeViewModel>();

        // MainWindow
        services.AddTransient<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host!.StartAsync();

        // Datenbanken für alle Mandanten erstellen falls nicht vorhanden
        var dbContextFactory = _host.Services.GetRequiredService<TenantDbContextFactory>();
        var tenantProvider = _host.Services.GetRequiredService<ITenantProvider>();
        var folderService = _host.Services.GetRequiredService<IFolderService>();
        
        foreach (var tenant in tenantProvider.AvailableTenants)
        {
            tenantProvider.CurrentTenant = tenant;
            await dbContextFactory.EnsureDatabaseCreatedAsync();
            
            // Root- und AutoImport-Ordner für jeden Mandanten anlegen
            await folderService.EnsureRootFolderExistsAsync();
            await folderService.EnsureSpecialFolderExistsAsync("AutoImport");
        }
        
        // Zurück zum ersten Mandanten
        tenantProvider.CurrentTenant = tenantProvider.AvailableTenants.FirstOrDefault() ?? "TenantA";

        // MainWindow und MainViewModel aus einem Scope holen
        // Der Scope bleibt für die Lebensdauer der App offen
        _appScope = _host.Services.CreateScope();
        var mainWindow = _appScope.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _appScope.ServiceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _appScope?.Dispose();
        
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
