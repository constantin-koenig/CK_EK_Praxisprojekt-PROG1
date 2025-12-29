using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ArchivSoftware.Ui.ViewModels;

namespace ArchivSoftware.Ui.Views;

/// <summary>
/// Dialog zur Auswahl eines Zielordners für Verschiebe-Operationen.
/// </summary>
public partial class FolderSelectionDialog : Window
{
    private readonly Guid _excludeFolderId;
    
    public FolderNodeViewModel? SelectedFolder { get; private set; }

    public FolderSelectionDialog(
        ObservableCollection<FolderNodeViewModel> folders, 
        Guid excludeFolderId,
        string title = "Zielordner auswählen")
    {
        InitializeComponent();
        
        _excludeFolderId = excludeFolderId;
        Title = title;
        
        // Kopiere den Baum und filtere den zu verschiebenden Ordner heraus
        var filteredFolders = FilterFolders(folders);
        FolderTreeView.ItemsSource = filteredFolders;
    }

    private ObservableCollection<FolderNodeViewModel> FilterFolders(ObservableCollection<FolderNodeViewModel> folders)
    {
        var result = new ObservableCollection<FolderNodeViewModel>();
        
        foreach (var folder in folders)
        {
            var filtered = FilterFolder(folder);
            if (filtered != null)
            {
                result.Add(filtered);
            }
        }
        
        return result;
    }

    private FolderNodeViewModel? FilterFolder(FolderNodeViewModel folder)
    {
        // Ordner selbst ausschließen (kann nicht in sich selbst verschoben werden)
        if (folder.Id == _excludeFolderId)
            return null;

        var copy = new FolderNodeViewModel
        {
            Id = folder.Id,
            Name = folder.Name,
            Parent = folder.Parent
        };

        foreach (var child in folder.Children)
        {
            var filteredChild = FilterFolder(child);
            if (filteredChild != null)
            {
                copy.Children.Add(filteredChild);
            }
        }

        return copy;
    }

    private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedFolder = e.NewValue as FolderNodeViewModel;
        OkButton.IsEnabled = SelectedFolder != null;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedFolder != null)
        {
            DialogResult = true;
            Close();
        }
    }
}
