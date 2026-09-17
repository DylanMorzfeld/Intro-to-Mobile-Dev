using System.Globalization;
using FitTrack.Models;

namespace FitTrack.Converters;

/// <summary>
/// Converts a FitnessGoal into a 0.0-1.0 progress value for a ProgressBar,
/// correctly handling both "increasing" goals (muscle gain, endurance -
/// progress means CurrentValue rising toward TargetValue) and "decreasing"
/// goals (weight loss - progress means CurrentValue falling toward
/// TargetValue).
/// </summary>
public class GoalProgressConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FitnessGoal goal)
            return 0.0;

        double progress;

        if (goal.IsDecreasingGoal)
        {
            // e.g. Start=200, Target=180, Current=190 -> 50% there.
            var totalToLose = goal.StartValue - goal.TargetValue;
            var lostSoFar = goal.StartValue - goal.CurrentValue;
            progress = totalToLose <= 0 ? 0.0 : lostSoFar / totalToLose;
        }
        else
        {
            // e.g. Start=0, Target=50, Current=25 -> 50% there.
            var totalToGain = goal.TargetValue - goal.StartValue;
            var gainedSoFar = goal.CurrentValue - goal.StartValue;
            progress = totalToGain <= 0 ? 0.0 : gainedSoFar / totalToGain;
        }

        return Math.Clamp(progress, 0.0, 1.0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}