using System.Collections.ObjectModel;
using ArchivSoftware.Ui.Services;

namespace ArchivSoftware.Ui.ViewModels;

/// <summary>
/// ViewModel für das Import-Log Fenster.
/// </summary>
public class ImportLogViewModel : ViewModelBase
{
    private string _importPath = string.Empty;
    private ObservableCollection<ImportLogItem> _importLog;

    public ImportLogViewModel(ObservableCollection<ImportLogItem> importLog, string importPath)
    {
        _importLog = importLog;
        _importPath = importPath;
    }

    public string ImportPath
    {
        get => _importPath;
        set => SetProperty(ref _importPath, value);
    }

    public ObservableCollection<ImportLogItem> ImportLog
    {
        get => _importLog;
        set => SetProperty(ref _importLog, value);
    }
}
