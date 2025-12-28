using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ArchivSoftware.Ui.Converters;

/// <summary>
/// Konvertiert eine Zahl größer 0 zu Visible, sonst Collapsed.
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
