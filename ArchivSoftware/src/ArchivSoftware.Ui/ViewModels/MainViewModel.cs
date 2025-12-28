using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;
using ArchivSoftware.Ui.Views;
using Microsoft.Win32;

namespace ArchivSoftware.Ui.ViewModels;

/// <summary>
/// Haupt-ViewModel für die Anwendung.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IFolderService _folderService;
    private readonly ISearchService _searchService;
    private readonly SemaphoreSlim _dbLock = new(1, 1);
    private ViewModelBase? _currentViewModel;
    private string _statusMessage = "Bereit";
    private FolderNodeViewModel? _selectedFolder;
    private ObservableCollection<FolderNodeViewModel> _folders = new();
    private ObservableCollection<DocumentDto> _documents = new();
    private DocumentDto? _selectedDocument;
    private bool _isInitialized = false;
    
    // Suche
    private string _searchTerm = string.Empty;
    private ObservableCollection<SearchResultDto> _searchResults = new();
    private SearchResultDto? _selectedSearchResult;
    private string _selectedDocumentText = string.Empty;
    private string _selectedDocumentTitle = string.Empty;

    // Import-Log (wird via DI injiziert)
    private ObservableCollection<ImportLogItem> _importLog;

    public MainViewModel(
        IDocumentService documentService, 
        IFolderService folderService, 
        ISearchService searchService,
        ObservableCollection<ImportLogItem> importLog)
    {
        _documentService = documentService;
        _folderService = folderService;
        _searchService = searchService;
        _importLog = importLog;

        ShowDocumentsCommand = new RelayCommand(() => { });
        ShowFoldersCommand = new RelayCommand(() => { });
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
        CreateFolderCommand = new RelayCommand(async () => await CreateFolderAsync());
        AddDocumentCommand = new RelayCommand(async () => await AddDocumentAsync());
        SearchCommand = new RelayCommand(async () => await SearchAsync());

        // Lade Ordner beim Start
        _ = InitializeAsync();
    }

    /// <summary>
    /// Import-Log für den ImportWatcher.
    /// </summary>
    public ObservableCollection<ImportLogItem> ImportLog
    {
        get => _importLog;
        set => SetProperty(ref _importLog, value);
    }

    public ObservableCollection<FolderNodeViewModel> Folders
    {
        get => _folders;
        set => SetProperty(ref _folders, value);
    }

    public ObservableCollection<DocumentDto> Documents
    {
        get => _documents;
        set => SetProperty(ref _documents, value);
    }

    public DocumentDto? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (SetProperty(ref _selectedDocument, value) && value != null)
            {
                _ = LoadSelectedDocumentAsync(value.Id);
            }
        }
    }

    public FolderNodeViewModel? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (SetProperty(ref _selectedFolder, value))
            {
                if (_isInitialized)
                {
                    _ = LoadDocumentsForFolderAsync();
                }
            }
        }
    }

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
    public ICommand AddDocumentCommand { get; }
    public ICommand SearchCommand { get; }

    // Suche Properties
    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public ObservableCollection<SearchResultDto> SearchResults
    {
        get => _searchResults;
        set => SetProperty(ref _searchResults, value);
    }

    public SearchResultDto? SelectedSearchResult
    {
        get => _selectedSearchResult;
        set
        {
            if (SetProperty(ref _selectedSearchResult, value) && value != null)
            {
                _ = LoadSelectedDocumentAsync(value.DocumentId);
            }
        }
    }

    public string SelectedDocumentText
    {
        get => _selectedDocumentText;
        set => SetProperty(ref _selectedDocumentText, value);
    }

    public string SelectedDocumentTitle
    {
        get => _selectedDocumentTitle;
        set => SetProperty(ref _selectedDocumentTitle, value);
    }

    private async Task InitializeAsync()
    {
        await _dbLock.WaitAsync();
        try
        {
            StatusMessage = "Initialisiere...";
            
            // Stelle sicher, dass Root-Ordner existiert
            await _folderService.EnsureRootFolderExistsAsync();

            // Stelle sicher, dass der Autoimport-Ordner existiert
            await _folderService.EnsureSpecialFolderExistsAsync("Autoimport");

            // Lade Ordnerbaum
            var folders = await _folderService.GetFolderTreeAsync();
            Folders = FolderNodeViewModel.FromFolders(folders);

            _isInitialized = true;
            StatusMessage = "Bereit";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler: {ex.Message}";
        }
        finally
        {
            _dbLock.Release();
        }
    }

    private async Task LoadFoldersAsync()
    {
        await _dbLock.WaitAsync();
        try
        {
            var folders = await _folderService.GetFolderTreeAsync();
            Folders = FolderNodeViewModel.FromFolders(folders);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Laden der Ordner: {ex.Message}";
        }
        finally
        {
            _dbLock.Release();
        }
    }

    private async Task RefreshAsync()
    {
        await _dbLock.WaitAsync();
        try
        {
            StatusMessage = "Aktualisiere...";
            
            var folders = await _folderService.GetFolderTreeAsync();
            Folders = FolderNodeViewModel.FromFolders(folders);
            
            StatusMessage = "Bereit";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Aktualisieren: {ex.Message}";
        }
        finally
        {
            _dbLock.Release();
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

        await _dbLock.WaitAsync();
        try
        {
            StatusMessage = "Erstelle Ordner...";

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
        finally
        {
            _dbLock.Release();
        }
    }

    private async Task LoadDocumentsForFolderAsync()
    {
        if (SelectedFolder == null)
        {
            Documents.Clear();
            return;
        }

        await _dbLock.WaitAsync();
        try
        {
            StatusMessage = $"Lade Dokumente für Ordner '{SelectedFolder.Name}'...";
            var documents = await _documentService.GetByFolderAsync(SelectedFolder.Id);
            var documentList = documents.ToList();
            
            Documents.Clear();
            foreach (var doc in documentList)
            {
                Documents.Add(doc);
            }
            
            StatusMessage = $"{Documents.Count} Dokument(e) in '{SelectedFolder.Name}' geladen";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Laden der Dokumente: {ex.Message}";
        }
        finally
        {
            _dbLock.Release();
        }
    }

    private async Task AddDocumentAsync()
    {
        // Prüfe ob ein Ordner ausgewählt ist
        if (SelectedFolder == null)
        {
            MessageBox.Show(
                "Bitte wählen Sie zuerst einen Ordner aus.",
                "Kein Ordner ausgewählt",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // OpenFileDialog öffnen
        var openFileDialog = new OpenFileDialog
        {
            Title = "Dokument importieren",
            Filter = "Alle unterstützten Dateien (*.txt;*.pdf)|*.txt;*.pdf|Textdateien (*.txt)|*.txt|PDF-Dateien (*.pdf)|*.pdf|Alle Dateien (*.*)|*.*",
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }

        var selectedFolderId = SelectedFolder.Id;
        var selectedFolderName = SelectedFolder.Name;

        await _dbLock.WaitAsync();
        try
        {
            StatusMessage = "Importiere Dokument...";

            var document = await _documentService.ImportFileAsync(selectedFolderId, openFileDialog.FileName);

            // Nach Import: Dokumente laden (innerhalb des Locks)
            var documents = await _documentService.GetByFolderAsync(selectedFolderId);
            var documentList = documents.ToList();
            
            Documents.Clear();
            foreach (var doc in documentList)
            {
                Documents.Add(doc);
            }

            StatusMessage = $"Dokument '{document.Title}' importiert - {Documents.Count} Dokument(e) in '{selectedFolderName}'";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler: {ex.Message}";
            MessageBox.Show(
                ex.Message,
                "Fehler beim Importieren",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _dbLock.Release();
        }
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            SearchResults.Clear();
            SelectedDocumentText = string.Empty;
            SelectedDocumentTitle = string.Empty;
            return;
        }

        await _dbLock.WaitAsync();
        try
        {
            StatusMessage = $"Suche nach '{SearchTerm}'...";
            
            var results = await _searchService.SearchAsync(SearchTerm);
            
            SearchResults.Clear();
            foreach (var result in results)
            {
                SearchResults.Add(result);
            }

            StatusMessage = $"{SearchResults.Count} Ergebnis(se) für '{SearchTerm}'";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler bei der Suche: {ex.Message}";
        }
        finally
        {
            _dbLock.Release();
        }
    }

    private async Task LoadSelectedDocumentAsync(Guid documentId)
    {
        await _dbLock.WaitAsync();
        try
        {
            StatusMessage = "Lade Dokument...";
            
            var documentWithData = await _documentService.GetWithDataAsync(documentId);
            
            if (documentWithData != null)
            {
                SelectedDocumentTitle = documentWithData.Title;
                SelectedDocumentText = documentWithData.PlainText ?? string.Empty;
                StatusMessage = $"Dokument '{documentWithData.Title}' geladen";
            }
            else
            {
                SelectedDocumentTitle = string.Empty;
                SelectedDocumentText = string.Empty;
                StatusMessage = "Dokument nicht gefunden";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Laden des Dokuments: {ex.Message}";
        }
        finally
        {
            _dbLock.Release();
        }
    }
}
