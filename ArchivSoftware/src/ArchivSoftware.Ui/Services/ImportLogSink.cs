using System.Collections.ObjectModel;
using System.Windows.Threading;
using ArchivSoftware.Application.Interfaces;

namespace ArchivSoftware.Ui.Services;

/// <summary>
/// Implementierung von IImportLogSink für die UI.
/// Schreibt via Dispatcher in eine ObservableCollection.
/// </summary>
public class ImportLogSink : IImportLogSink
{
    private readonly ObservableCollection<ViewModels.ImportLogItem> _importLog;
    private readonly Dispatcher _dispatcher;

    public ImportLogSink(ObservableCollection<ViewModels.ImportLogItem> importLog)
    {
        _importLog = importLog;
        _dispatcher = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }

    public void Add(string fileName, ImportStatus status, string message)
    {
        var logItem = new ViewModels.ImportLogItem
        {
            Timestamp = DateTime.Now,
            FileName = fileName,
            Status = status,
            Message = message
        };

        if (_dispatcher.CheckAccess())
        {
            _importLog.Add(logItem);
        }
        else
        {
            _dispatcher.BeginInvoke(() => _importLog.Add(logItem));
        }
    }
}
