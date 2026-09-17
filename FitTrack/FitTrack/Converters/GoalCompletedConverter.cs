using System.Globalization;
using FitTrack.Models;

namespace FitTrack.Converters;

/// <summary>
/// Returns true when a FitnessGoal's progress has reached 100%, using the
/// same increasing/decreasing goal logic as GoalProgressConverter. Kept as
/// its own converter (rather than reusing GoalProgressConverter's numeric
/// output) so the View can bind directly to a boolean for IsVisible,
/// rather than doing a >= comparison inside XAML.
/// </summary>
public class GoalCompletedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FitnessGoal goal)
            return false;

        return goal.IsDecreasingGoal
            ? goal.CurrentValue <= goal.TargetValue
            : goal.CurrentValue >= goal.TargetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}