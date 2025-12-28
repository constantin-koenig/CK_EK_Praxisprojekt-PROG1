using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Ui.Views;

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
        CreateFolderCommand = new RelayCommand(async () => await CreateFolderAsync());

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
    public ICommand CreateFolderCommand { get; }

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
        try
        {
            var folders = await _folderService.GetFolderTreeAsync();
            Folders = FolderNodeViewModel.FromFolders(folders);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Laden der Ordner: {ex.Message}";
        }
    }

    private async Task RefreshAsync()
    {
        try
        {
            StatusMessage = "Aktualisiere...";
            await LoadFoldersAsync();
            await DocumentListViewModel.LoadDocumentsAsync();
            await FolderTreeViewModel.LoadFoldersAsync();
            StatusMessage = "Bereit";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Aktualisieren: {ex.Message}";
        }
    }

    private async Task CreateFolderAsync()
    {
        // Dialog für Ordnernamen anzeigen
        var folderName = InputDialog.Show(
            "Neuer Ordner",
            "Geben Sie den Namen für den neuen Ordner ein:",
            "",
            System.Windows.Application.Current.MainWindow);

        if (string.IsNullOrWhiteSpace(folderName))
        {
            return;
        }

        try
        {
            StatusMessage = "Erstelle Ordner...";

            // Übergeordneten Ordner bestimmen
            Guid? parentFolderId = SelectedFolder?.Id;

            // Wenn kein Ordner ausgewählt, verwende Root
            if (parentFolderId == null && Folders.Count > 0)
            {
                parentFolderId = Folders[0].Id;
            }

            if (parentFolderId == null)
            {
                StatusMessage = "Fehler: Kein übergeordneter Ordner verfügbar";
                return;
            }

            // Ordner erstellen
            var newFolder = await _folderService.CreateFolderAsync(parentFolderId.Value, folderName);

            // Neuen Ordner zum ViewModel hinzufügen
            var newNode = new FolderNodeViewModel
            {
                Id = newFolder.Id,
                Name = newFolder.Name
            };

            if (SelectedFolder != null)
            {
                newNode.Parent = SelectedFolder;
                SelectedFolder.Children.Add(newNode);
                SelectedFolder.IsExpanded = true;
            }
            else if (Folders.Count > 0)
            {
                newNode.Parent = Folders[0];
                Folders[0].Children.Add(newNode);
                Folders[0].IsExpanded = true;
            }

            StatusMessage = $"Ordner '{folderName}' erstellt";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler: {ex.Message}";
            MessageBox.Show(
                ex.Message,
                "Fehler beim Erstellen des Ordners",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
