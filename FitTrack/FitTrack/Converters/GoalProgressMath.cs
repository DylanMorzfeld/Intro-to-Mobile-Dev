using FitTrack.Models;

namespace FitTrack.Converters;

/// <summary>
/// Shared progress calculation used by both GoalProgressConverter (the bar's
/// fill amount) and GoalProgressColorConverter (the bar's color), so the two
/// never drift out of sync with each other.
/// </summary>
public static class GoalProgressMath
{
    public static double Calculate(FitnessGoal goal)
    {
        double progress;

        if (goal.IsDecreasingGoal)
        {
            var totalToLose = goal.StartValue - goal.TargetValue;
            var lostSoFar = goal.StartValue - goal.CurrentValue;
            progress = totalToLose <= 0 ? 0.0 : lostSoFar / totalToLose;
        }
        else
        {
            var totalToGain = goal.TargetValue - goal.StartValue;
            var gainedSoFar = goal.CurrentValue - goal.StartValue;
            progress = totalToGain <= 0 ? 0.0 : gainedSoFar / totalToGain;
        }

        return Math.Clamp(progress, 0.0, 1.0);
    }
}