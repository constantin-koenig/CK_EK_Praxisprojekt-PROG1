using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Application.Services;
using ArchivSoftware.Domain.Interfaces;
using ArchivSoftware.Infrastructure;
using ArchivSoftware.Infrastructure.Data;
using ArchivSoftware.Infrastructure.Repositories;
using ArchivSoftware.Ui.ViewModels;
using ArchivSoftware.Ui.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace ArchivSoftware.Ui;

/// <summary>
/// Hauptanwendungsklasse mit Host-basierter Dependency Injection.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

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
        // Connection String aus appsettings.json
        var connectionString = configuration.GetConnectionString("ArchivSoftwareDb");

        // DbContext mit SQL Server
        services.AddDbContext<ArchivSoftwareDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories als Scoped
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Application Services
        services.AddScoped<IFolderService, FolderService>();
        services.AddScoped<IDocumentService, DocumentService>();

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

        // Datenbank erstellen falls nicht vorhanden
        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArchivSoftwareDbContext>();
        await context.Database.EnsureCreatedAsync();

        // MainWindow via DI öffnen
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
