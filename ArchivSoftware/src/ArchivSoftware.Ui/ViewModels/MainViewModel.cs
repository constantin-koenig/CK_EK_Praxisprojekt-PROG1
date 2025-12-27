using System.Windows.Input;
using ArchivSoftware.Application.Interfaces;

namespace ArchivSoftware.Ui.ViewModels;

/// <summary>
/// Haupt-ViewModel für die Anwendung.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly ICategoryService _categoryService;
    private ViewModelBase? _currentViewModel;
    private string _statusMessage = "Bereit";

    public MainViewModel(IDocumentService documentService, ICategoryService categoryService)
    {
        _documentService = documentService;
        _categoryService = categoryService;

        DocumentListViewModel = new DocumentListViewModel(documentService);
        CategoryTreeViewModel = new CategoryTreeViewModel(categoryService);

        CurrentViewModel = DocumentListViewModel;

        ShowDocumentsCommand = new RelayCommand(() => CurrentViewModel = DocumentListViewModel);
        ShowCategoriesCommand = new RelayCommand(() => CurrentViewModel = CategoryTreeViewModel);
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
    }

    public DocumentListViewModel DocumentListViewModel { get; }
    public CategoryTreeViewModel CategoryTreeViewModel { get; }

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
    public ICommand ShowCategoriesCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task RefreshAsync()
    {
        StatusMessage = "Aktualisiere...";
        await DocumentListViewModel.LoadDocumentsAsync();
        await CategoryTreeViewModel.LoadCategoriesAsync();
        StatusMessage = "Bereit";
    }
}
