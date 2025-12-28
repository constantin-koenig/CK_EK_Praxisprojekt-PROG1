using System.Windows;

namespace ArchivSoftware.Ui.Views;

/// <summary>
/// Einfacher Eingabedialog für Textinput.
/// </summary>
public partial class InputDialog : Window
{
    public InputDialog(string title, string prompt, string defaultValue = "")
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = prompt;
        InputTextBox.Text = defaultValue;
        InputTextBox.SelectAll();
        InputTextBox.Focus();
    }

    public string InputValue => InputTextBox.Text;

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// Zeigt den Dialog und gibt den eingegebenen Wert zurück.
    /// </summary>
    public static string? Show(string title, string prompt, string defaultValue = "", Window? owner = null)
    {
        var dialog = new InputDialog(title, prompt, defaultValue);
        if (owner != null)
        {
            dialog.Owner = owner;
        }
        
        if (dialog.ShowDialog() == true)
        {
            return dialog.InputValue;
        }
        return null;
    }
}
