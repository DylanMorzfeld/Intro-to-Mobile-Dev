using System.Globalization;

namespace FitTrack.Converters;

/// <summary>
/// Flips a SwipeItem's label between "Complete" and "Undo" depending on
/// whether the workout is already marked completed - keeps that small
/// piece of display logic out of the ViewModel.
/// </summary>
public class CompleteSwipeTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "Undo" : "Complete";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}