using System.Collections.ObjectModel;
using ArchivSoftware.Domain.Entities;

namespace ArchivSoftware.Ui.ViewModels;

/// <summary>
/// ViewModel für einen Ordner-Knoten im Baum.
/// </summary>
public class FolderNodeViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private Guid _id;
    private Guid? _parentFolderId;
    private bool _isExpanded;
    private bool _isSelected;

    public FolderNodeViewModel()
    {
        Children = new ObservableCollection<FolderNodeViewModel>();
    }

    public FolderNodeViewModel(Folder folder) : this()
    {
        Id = folder.Id;
        Name = folder.Name;
        ParentFolderId = folder.ParentFolderId;

        foreach (var child in folder.Children)
        {
            Children.Add(new FolderNodeViewModel(child));
        }
    }

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public Guid? ParentFolderId
    {
        get => _parentFolderId;
        set => SetProperty(ref _parentFolderId, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public ObservableCollection<FolderNodeViewModel> Children { get; }

    /// <summary>
    /// Erstellt eine Liste von FolderNodeViewModels aus einer Liste von Folder-Entities.
    /// </summary>
    public static ObservableCollection<FolderNodeViewModel> FromFolders(IEnumerable<Folder> folders)
    {
        var result = new ObservableCollection<FolderNodeViewModel>();
        foreach (var folder in folders)
        {
            result.Add(new FolderNodeViewModel(folder));
        }
        return result;
    }
}
