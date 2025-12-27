using System.Collections.ObjectModel;
using System.Windows.Input;
using ArchivSoftware.Application.DTOs;
using ArchivSoftware.Application.Interfaces;

namespace ArchivSoftware.Ui.ViewModels;

/// <summary>
/// ViewModel für den Ordnerbaum.
/// </summary>
public class FolderTreeViewModel : ViewModelBase
{
    private readonly IFolderService _folderService;
    private ObservableCollection<FolderTreeDto> _folders = new();
    private FolderTreeDto? _selectedFolder;
    private bool _isLoading;

    public FolderTreeViewModel(IFolderService folderService)
    {
        _folderService = folderService;
        
        LoadFoldersCommand = new RelayCommand(async () => await LoadFoldersAsync());
        CreateFolderCommand = new RelayCommand<string>(async name => await CreateFolderAsync(name));
    }

    public ObservableCollection<FolderTreeDto> Folders
    {
        get => _folders;
        set => SetProperty(ref _folders, value);
    }

    public FolderTreeDto? SelectedFolder
    {
        get => _selectedFolder;
        set => SetProperty(ref _selectedFolder, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand LoadFoldersCommand { get; }
    public ICommand CreateFolderCommand { get; }

    public event Action<Guid?>? FolderSelected;

    public async Task LoadFoldersAsync()
    {
        IsLoading = true;
        try
        {
            var folders = await _folderService.GetFolderTreeAsync();
            Folders = new ObservableCollection<FolderTreeDto>(folders);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OnFolderSelected(FolderTreeDto? folder)
    {
        SelectedFolder = folder;
        FolderSelected?.Invoke(folder?.Id);
    }

    private async Task CreateFolderAsync(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        try
        {
            var dto = new CreateFolderDto(name, SelectedFolder?.Id);
            await _folderService.CreateAsync(dto);
            await LoadFoldersAsync();
        }
        catch (Exception ex)
        {
            // In einer echten Anwendung würde hier eine Fehlermeldung angezeigt werden
            System.Diagnostics.Debug.WriteLine($"Fehler beim Erstellen des Ordners: {ex.Message}");
        }
    }
}
