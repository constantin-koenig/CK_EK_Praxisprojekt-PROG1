using System.Windows;
using System.Windows.Controls;
using ArchivSoftware.Ui.ViewModels;

namespace ArchivSoftware.Ui.Views;

/// <summary>
/// Interaktionslogik für MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel viewModel && e.NewValue is FolderNodeViewModel folder)
        {
            viewModel.SelectedFolder = folder;
        }
    }
}
