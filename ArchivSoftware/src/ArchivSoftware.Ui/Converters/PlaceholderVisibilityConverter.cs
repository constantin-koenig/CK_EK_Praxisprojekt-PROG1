using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ArchivSoftware.Ui.Converters;

/// <summary>
/// Zeigt Visibility.Visible wenn SelectedFolder null ist UND SearchResults.Count == 0.
/// </summary>
public class PlaceholderVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] = SelectedFolder
        // values[1] = SearchResults.Count
        
        if (values.Length < 2)
            return Visibility.Collapsed;

        var selectedFolder = values[0];
        var searchResultsCount = values[1] is int count ? count : 0;

        // Zeige Platzhalter nur wenn kein Ordner ausgewählt UND keine Suchergebnisse
        if (selectedFolder == null && searchResultsCount == 0)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
