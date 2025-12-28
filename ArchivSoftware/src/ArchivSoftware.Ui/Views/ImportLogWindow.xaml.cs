using System.Windows;

namespace ArchivSoftware.Ui.Views;

/// <summary>
/// Fenster zur Anzeige des Auto-Import Logs.
/// </summary>
public partial class ImportLogWindow : Window
{
    public ImportLogWindow()
    {
        InitializeComponent();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
