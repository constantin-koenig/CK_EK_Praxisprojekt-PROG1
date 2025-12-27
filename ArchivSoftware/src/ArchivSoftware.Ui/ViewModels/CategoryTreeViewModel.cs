using System.Collections.ObjectModel;
using System.Windows.Input;
using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;

namespace ArchivSoftware.Ui.ViewModels;

/// <summary>
/// ViewModel für den Kategoriebaum.
/// </summary>
public class CategoryTreeViewModel : ViewModelBase
{
    private readonly ICategoryService _categoryService;
    private ObservableCollection<CategoryDto> _categories = new();
    private CategoryDto? _selectedCategory;
    private bool _isLoading;

    public CategoryTreeViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        
        LoadCategoriesCommand = new RelayCommand(async () => await LoadCategoriesAsync());
    }

    public ObservableCollection<CategoryDto> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public CategoryDto? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand LoadCategoriesCommand { get; }

    public async Task LoadCategoriesAsync()
    {
        IsLoading = true;
        try
        {
            var categories = await _categoryService.GetRootCategoriesAsync();
            Categories = new ObservableCollection<CategoryDto>(categories);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
