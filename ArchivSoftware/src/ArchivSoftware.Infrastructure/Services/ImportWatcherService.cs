using System.Threading.Channels;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArchivSoftware.Infrastructure.Services;

/// <summary>
/// Background-Service der einen Ordner überwacht und neue Dateien automatisch importiert.
/// </summary>
public class ImportWatcherService : BackgroundService
{
    private readonly ImportWatcherOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImportWatcherService> _logger;
    private readonly IImportLogSink? _importLogSink;
    private readonly Channel<string> _fileChannel;
    private FileSystemWatcher? _watcher;

    private static readonly string[] AllowedExtensions = { ".pdf", ".txt" };
    private const int MaxRetries = 10;
    private const int RetryDelayMs = 500;

    public ImportWatcherService(
        IOptions<ImportWatcherOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<ImportWatcherService> logger,
        IImportLogSink? importLogSink = null)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _importLogSink = importLogSink;
        _fileChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Prüfe ob aktiviert
        if (!_options.Enabled)
        {
            _logger.LogInformation("ImportWatcher ist deaktiviert");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Path))
        {
            _logger.LogWarning("ImportWatcher: Kein Pfad konfiguriert");
            return;
        }

        // Stelle sicher, dass der Überwachungsordner existiert
        if (!Directory.Exists(_options.Path))
        {
            try
            {
                Directory.CreateDirectory(_options.Path);
                _logger.LogInformation("ImportWatcher: Ordner erstellt: {Path}", _options.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportWatcher: Konnte Ordner nicht erstellen: {Path}", _options.Path);
                return;
            }
        }

        // FileSystemWatcher einrichten
        _watcher = new FileSystemWatcher(_options.Path)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
            EnableRaisingEvents = true
        };

        _watcher.Created += OnFileDetected;
        _watcher.Renamed += OnFileDetected;

        _logger.LogInformation("ImportWatcher gestartet. Überwache: {Path}", _options.Path);
        _importLogSink?.Add("System", ImportStatus.Started, $"Überwache Ordner: {_options.Path}");

        // Background-Loop für Import
        await ProcessFilesAsync(stoppingToken);
    }

    private void OnFileDetected(object sender, FileSystemEventArgs e)
    {
        var extension = Path.GetExtension(e.FullPath).ToLowerInvariant();
        var fileName = Path.GetFileName(e.FullPath);

        if (!AllowedExtensions.Contains(extension))
        {
            _logger.LogDebug("ImportWatcher: Datei ignoriert (nicht unterstützt): {FileName}", e.Name);
            _importLogSink?.Add(fileName, ImportStatus.Ignored, $"Dateityp {extension} nicht unterstützt");
            return;
        }

        _logger.LogInformation("ImportWatcher: Neue Datei erkannt: {FileName}", e.Name);
        _importLogSink?.Add(fileName, ImportStatus.Started, "Datei erkannt, Import wird vorbereitet");
        _fileChannel.Writer.TryWrite(e.FullPath);
    }

    private async Task ProcessFilesAsync(CancellationToken stoppingToken)
    {
        await foreach (var filePath in _fileChannel.Reader.ReadAllAsync(stoppingToken))
        {
            var fileName = Path.GetFileName(filePath);
            try
            {
                // Warte bis Datei fertig geschrieben ist
                if (!await WaitForFileReadyAsync(filePath, stoppingToken))
                {
                    _logger.LogWarning("ImportWatcher: Datei nicht zugänglich nach {Retries} Versuchen: {FilePath}", 
                        MaxRetries, filePath);
                    _importLogSink?.Add(fileName, ImportStatus.Failed, $"Datei nicht zugänglich nach {MaxRetries} Versuchen");
                    continue;
                }

                // Importiere die Datei
                await ImportFileAsync(filePath, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportWatcher: Fehler beim Verarbeiten von {FilePath}", filePath);
                _importLogSink?.Add(fileName, ImportStatus.Failed, ex.Message);
            }
        }
    }

    private async Task<bool> WaitForFileReadyAsync(string filePath, CancellationToken stoppingToken)
    {
        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                // Versuche Datei exklusiv zu öffnen
                using var stream = new FileStream(
                    filePath, 
                    FileMode.Open, 
                    FileAccess.Read, 
                    FileShare.None);
                
                return true; // Datei ist bereit
            }
            catch (IOException)
            {
                // Datei wird noch geschrieben
                _logger.LogDebug("ImportWatcher: Datei noch nicht bereit, Versuch {Attempt}/{MaxRetries}: {FilePath}", 
                    i + 1, MaxRetries, filePath);
                await Task.Delay(RetryDelayMs, stoppingToken);
            }
            catch (UnauthorizedAccessException)
            {
                // Keine Berechtigung
                _logger.LogWarning("ImportWatcher: Keine Berechtigung für Datei: {FilePath}", filePath);
                return false;
            }
        }

        return false;
    }

    private async Task ImportFileAsync(string filePath, CancellationToken stoppingToken)
    {
        var fileName = Path.GetFileName(filePath);
        using var scope = _scopeFactory.CreateScope();
        var documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();
        var folderService = scope.ServiceProvider.GetRequiredService<IFolderService>();

        try
        {
            // Stelle sicher, dass der Zielordner existiert
            var targetFolderId = await folderService.EnsureSpecialFolderExistsAsync(
                _options.TargetFolder, 
                stoppingToken);

            // Importiere das Dokument
            var document = await documentService.ImportFileAsync(targetFolderId, filePath, stoppingToken);
            
            _logger.LogInformation("ImportWatcher: Dokument importiert: {Title} -> {TargetFolder}", 
                document.Title, _options.TargetFolder);
            _importLogSink?.Add(fileName, ImportStatus.Imported, $"Erfolgreich importiert nach {_options.TargetFolder}");

            // Lösche die Quelldatei nach erfolgreichem Import
            try
            {
                File.Delete(filePath);
                _logger.LogDebug("ImportWatcher: Quelldatei gelöscht: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ImportWatcher: Konnte Quelldatei nicht löschen: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ImportWatcher: Fehler beim Import von {FilePath}", filePath);
            _importLogSink?.Add(fileName, ImportStatus.Failed, ex.Message);
        }
    }

    public override void Dispose()
    {
        _watcher?.Dispose();
        _fileChannel.Writer.Complete();
        base.Dispose();
    }
}
