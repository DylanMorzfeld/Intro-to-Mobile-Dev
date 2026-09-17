using System.Globalization;

namespace FitTrack.Converters;

/// <summary>
/// Converts a string to true if it has content, false if null/empty.
/// Used to show/hide validation messages and similar conditional UI
/// without needing a boolean property on every ViewModel for each message.
/// </summary>
public class IsNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value as string);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}