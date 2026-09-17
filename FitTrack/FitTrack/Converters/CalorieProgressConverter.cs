using System.Globalization;

namespace FitTrack.Converters;

/// <summary>
/// Converts (TotalCalories, CalorieGoal) into a 0.0-1.0 progress value for
/// a ProgressBar.
///
/// This logic belongs here, not in the ViewModel or scattered as inline
/// math in XAML - a converter is the MVVM-correct place for "how do I
/// display this data" logic that doesn't affect the underlying data itself.
/// </summary>
public class CalorieProgressConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not { Length: 2 })
            return 0.0;

        if (values[0] is not int totalCalories || values[1] is not int calorieGoal)
            return 0.0;

        if (calorieGoal <= 0)
            return 0.0;

        // Clamp to 1.0 so the bar doesn't visually overflow if the user
        // has exceeded their goal for the day.
        var progress = (double)totalCalories / calorieGoal;
        return Math.Clamp(progress, 0.0, 1.0);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        // One-way conversion only - a ProgressBar's value is never edited
        // by the user, so converting back doesn't make sense here.
        throw new NotSupportedException();
    }
}