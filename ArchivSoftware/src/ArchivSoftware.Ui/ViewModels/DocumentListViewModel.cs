using System.Collections.ObjectModel;
using System.Windows.Input;
using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;

namespace ArchivSoftware.Ui.ViewModels;

/// <summary>
/// ViewModel für die Dokumentenliste.
/// </summary>
public class DocumentListViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private ObservableCollection<DocumentDto> _documents = new();
    private DocumentDto? _selectedDocument;
    private string _searchText = string.Empty;
    private bool _isLoading;
    private Guid? _currentFolderId;

    public DocumentListViewModel(IDocumentService documentService)
    {
        _documentService = documentService;
        
        LoadDocumentsCommand = new RelayCommand(async () => await LoadDocumentsAsync());
        SearchCommand = new RelayCommand(async () => await SearchDocumentsAsync());
        DeleteCommand = new RelayCommand(async () => await DeleteDocumentAsync(), () => SelectedDocument is not null);
    }

    public ObservableCollection<DocumentDto> Documents
    {
        get => _documents;
        set => SetProperty(ref _documents, value);
    }

    public DocumentDto? SelectedDocument
    {
        get => _selectedDocument;
        set => SetProperty(ref _selectedDocument, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public Guid? CurrentFolderId
    {
        get => _currentFolderId;
        set
        {
            if (SetProperty(ref _currentFolderId, value))
            {
                _ = LoadDocumentsAsync();
            }
        }
    }

    public ICommand LoadDocumentsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand DeleteCommand { get; }

    public async Task LoadDocumentsAsync()
    {
        IsLoading = true;
        try
        {
            IEnumerable<DocumentDto> documents;
            if (_currentFolderId.HasValue)
            {
                documents = await _documentService.GetByFolderAsync(_currentFolderId.Value);
            }
            else
            {
                documents = await _documentService.GetAllAsync();
            }
            Documents = new ObservableCollection<DocumentDto>(documents);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SearchDocumentsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadDocumentsAsync();
            return;
        }

        IsLoading = true;
        try
        {
            var documents = await _documentService.SearchAsync(SearchText);
            Documents = new ObservableCollection<DocumentDto>(documents);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteDocumentAsync()
    {
        if (SelectedDocument is null) return;

        await _documentService.DeleteAsync(SelectedDocument.Id);
        Documents.Remove(SelectedDocument);
        SelectedDocument = null;
    }
}
