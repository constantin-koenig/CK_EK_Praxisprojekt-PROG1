using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ArchivSoftware.Ui.Converters;

/// <summary>
/// Konvertiert null zu Visible, nicht-null zu Collapsed.
/// Nützlich um Platzhalter-Texte anzuzeigen wenn kein Wert vorhanden ist.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Wenn value null ist -> Visible (Platzhalter anzeigen)
        // Wenn value nicht null ist -> Collapsed (Platzhalter verstecken)
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
