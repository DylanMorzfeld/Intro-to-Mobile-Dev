using System.Globalization;
using FitTrack.Models;

namespace FitTrack.Converters;

/// <summary>
/// Picks a progress-bar color based on how close a goal is to completion,
/// looking the actual Color values up from the app's live resource
/// dictionary (GoalProgressLow/Mid/High in Colors.xaml) rather than
/// hardcoding them here. This is what makes the coloring "dynamic": the
/// visual cue is computed from the goal's live data every time the
/// converter runs, not fixed at design time.
/// </summary>
public class GoalProgressColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FitnessGoal goal)
            return Colors.Gray;

        double progress = GoalProgressMath.Calculate(goal);

        string resourceKey = progress switch
        {
            >= 1.0 => "GoalProgressHigh",
            >= 0.5 => "GoalProgressMid",
            _ => "GoalProgressLow"
        };

        if (Application.Current?.Resources.TryGetValue(resourceKey, out var color) == true)
            return color;

        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}