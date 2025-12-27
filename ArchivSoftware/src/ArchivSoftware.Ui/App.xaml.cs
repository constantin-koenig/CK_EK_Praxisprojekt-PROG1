using ArchivSoftware.Infrastructure;
using ArchivSoftware.Infrastructure.Data;
using ArchivSoftware.Ui.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ArchivSoftware.Ui;

/// <summary>
/// Hauptanwendungsklasse.
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Verwende SQLite für einfachere Entwicklung
        var connectionString = "Data Source=archiv.db";
        
        services.AddDbContext<ArchivDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddInfrastructure(connectionString);
        services.AddApplicationServices();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<DocumentListViewModel>();
        services.AddTransient<CategoryTreeViewModel>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Datenbank erstellen falls nicht vorhanden
        using var scope = _serviceProvider!.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArchivDbContext>();
        context.Database.EnsureCreated();

        var mainWindow = new Views.MainWindow
        {
            DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
