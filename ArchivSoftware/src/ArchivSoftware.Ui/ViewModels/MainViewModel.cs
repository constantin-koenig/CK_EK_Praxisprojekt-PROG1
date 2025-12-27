using System.Windows.Input;
using ArchivSoftware.Application.Interfaces;

namespace ArchivSoftware.Ui.ViewModels;

/// <summary>
/// Haupt-ViewModel für die Anwendung.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IFolderService _folderService;
    private ViewModelBase? _currentViewModel;
    private string _statusMessage = "Bereit";

    public MainViewModel(IDocumentService documentService, IFolderService folderService)
    {
        _documentService = documentService;
        _folderService = folderService;

        DocumentListViewModel = new DocumentListViewModel(documentService);
        FolderTreeViewModel = new FolderTreeViewModel(folderService);

        // Verbinde Ordner-Auswahl mit Dokumentenliste
        FolderTreeViewModel.FolderSelected += folderId =>
        {
            DocumentListViewModel.CurrentFolderId = folderId;
        };

        CurrentViewModel = DocumentListViewModel;

        ShowDocumentsCommand = new RelayCommand(() => CurrentViewModel = DocumentListViewModel);
        ShowFoldersCommand = new RelayCommand(() => CurrentViewModel = FolderTreeViewModel);
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
    }

    public DocumentListViewModel DocumentListViewModel { get; }
    public FolderTreeViewModel FolderTreeViewModel { get; }

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand ShowDocumentsCommand { get; }
    public ICommand ShowFoldersCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task RefreshAsync()
    {
        StatusMessage = "Aktualisiere...";
        await DocumentListViewModel.LoadDocumentsAsync();
        await FolderTreeViewModel.LoadFoldersAsync();
        StatusMessage = "Bereit";
    }
}
