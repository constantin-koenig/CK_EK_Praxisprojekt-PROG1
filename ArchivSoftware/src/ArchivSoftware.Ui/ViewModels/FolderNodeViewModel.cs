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
    private FolderNodeViewModel? _parent;
    private bool _isExpanded;
    private bool _isSelected;

    public FolderNodeViewModel()
    {
        Children = new ObservableCollection<FolderNodeViewModel>();
    }

    public FolderNodeViewModel(Folder folder, FolderNodeViewModel? parent = null) : this()
    {
        Id = folder.Id;
        Name = folder.Name;
        Parent = parent;

        foreach (var child in folder.Children)
        {
            Children.Add(new FolderNodeViewModel(child, this));
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

    public FolderNodeViewModel? Parent
    {
        get => _parent;
        set => SetProperty(ref _parent, value);
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

    /// <summary>
    /// Icon für den Ordner basierend auf dem Namen.
    /// </summary>
    public string Icon
    {
        get
        {
            // Root-Ordner
            if (Parent == null && Name.Equals("Root", StringComparison.OrdinalIgnoreCase))
                return "🏠";
            
            // Autoimport-Ordner
            if (Name.Equals("Autoimport", StringComparison.OrdinalIgnoreCase))
                return "📥";
            
            // Standard-Ordner
            return "📁";
        }
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
            result.Add(new FolderNodeViewModel(folder, null));
        }
        return result;
    }
}
