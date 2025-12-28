using System.Collections.ObjectModel;
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
    private FolderNodeViewModel? _selectedFolder;
    private ObservableCollection<FolderNodeViewModel> _folders = new();

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

        // Lade Ordner beim Start
        _ = InitializeAsync();
    }

    public ObservableCollection<FolderNodeViewModel> Folders
    {
        get => _folders;
        set => SetProperty(ref _folders, value);
    }

    public FolderNodeViewModel? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (SetProperty(ref _selectedFolder, value))
            {
                DocumentListViewModel.CurrentFolderId = value?.Id;
            }
        }
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

    private async Task InitializeAsync()
    {
        StatusMessage = "Initialisiere...";
        try
        {
            // Stelle sicher, dass Root-Ordner existiert
            await _folderService.EnsureRootFolderExistsAsync();

            // Lade Ordnerbaum
            await LoadFoldersAsync();

            StatusMessage = "Bereit";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }

    private async Task LoadFoldersAsync()
    {
        var folders = await _folderService.GetFolderTreeAsync();
        Folders = FolderNodeViewModel.FromFolders(folders);
    }

    private async Task RefreshAsync()
    {
        StatusMessage = "Aktualisiere...";
        await LoadFoldersAsync();
        await DocumentListViewModel.LoadDocumentsAsync();
        await FolderTreeViewModel.LoadFoldersAsync();
        StatusMessage = "Bereit";
    }
}
