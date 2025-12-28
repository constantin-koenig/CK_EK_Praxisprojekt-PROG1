using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using ArchivSoftware.Ui.Helpers;
using ArchivSoftware.Ui.Services;
using ArchivSoftware.Ui.ViewModels;
using Microsoft.Extensions.Configuration;

namespace ArchivSoftware.Ui.Views;

/// <summary>
/// Interaktionslogik für MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyPropertyChanged oldVm)
        {
            oldVm.PropertyChanged -= ViewModel_PropertyChanged;
        }

        if (e.NewValue is INotifyPropertyChanged newVm)
        {
            newVm.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedDocumentText) ||
            e.PropertyName == nameof(MainViewModel.SearchTerm))
        {
            UpdateHighlightedText();
        }
    }

    private void UpdateHighlightedText()
    {
        if (DataContext is not MainViewModel vm)
            return;

        DocumentPreviewTextBlock.Inlines.Clear();

        if (string.IsNullOrEmpty(vm.SelectedDocumentText))
            return;

        var inlines = TextHighlightHelper.BuildHighlightedInlines(vm.SelectedDocumentText, vm.SearchTerm);
        foreach (var inline in inlines)
        {
            DocumentPreviewTextBlock.Inlines.Add(inline);
        }
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel viewModel && e.NewValue is FolderNodeViewModel folder)
        {
            viewModel.SelectedFolder = folder;
        }
    }

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is MainViewModel viewModel)
        {
            viewModel.SearchCommand.Execute(null);
        }
    }

    private void ShowImportLog_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            var importPath = "C:\\ArchivSoftware\\Import"; // Default
            var logVm = new ImportLogViewModel(vm.ImportLog, importPath);
            
            var window = new ImportLogWindow
            {
                DataContext = logVm,
                Owner = this
            };
            window.Show();
        }
    }
}
