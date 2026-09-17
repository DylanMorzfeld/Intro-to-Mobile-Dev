using SQLite;

namespace FitTrack.Models;

/// <summary>
/// The category of fitness goal a user is working toward. Each type
/// implies a different "direction" of progress - see IsDecreasingGoal below.
/// </summary>
public enum GoalType
{
    WeightLoss,
    MuscleGain,
    Endurance
}

/// <summary>
/// Represents a single fitness goal with a measurable start, current, and
/// target value (e.g. weight in lbs, max lift in lbs, or run distance in
/// miles - the unit is just a label for display, not enforced by the type).
///
/// Plain data class, same as Workout and Meal - no MAUI or persistence
/// code, so it can flow unchanged through the repository, ViewModel, and
/// View bindings.
/// </summary>
public class FitnessGoal
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public GoalType GoalType { get; set; }

    /// <summary>Short user-facing label, e.g. "Lose 10 lbs before spring break".</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>The measurement where the user started (e.g. starting weight).</summary>
    public double StartValue { get; set; }

    /// <summary>The most recently logged measurement toward this goal.</summary>
    public double CurrentValue { get; set; }

    /// <summary>The measurement that represents the goal being achieved.</summary>
    public double TargetValue { get; set; }

    /// <summary>Unit label for display only, e.g. "lbs", "miles", "minutes".</summary>
    public string Unit { get; set; } = string.Empty;

    public DateTime StartDate { get; set; } = DateTime.Now;

    /// <summary>Optional target completion date.</summary>
    public DateTime? TargetDate { get; set; }

    /// <summary>
    /// True for goal types where progress means the value going DOWN
    /// (weight loss) rather than up (muscle gain, endurance). Used by the
    /// progress converter to calculate percent-complete correctly regardless
    /// of goal direction.
    /// </summary>
    public bool IsDecreasingGoal => GoalType == GoalType.WeightLoss;
}