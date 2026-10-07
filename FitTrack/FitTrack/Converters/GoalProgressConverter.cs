using System.Globalization;
using FitTrack.Models;

namespace FitTrack.Converters;

public class GoalProgressConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is FitnessGoal goal ? GoalProgressMath.Calculate(goal) : 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}