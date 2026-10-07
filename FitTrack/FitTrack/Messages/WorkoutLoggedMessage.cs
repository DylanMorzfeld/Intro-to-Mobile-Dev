using FitTrack.Models;

namespace FitTrack.Messages;

/// <summary>What the user did to the workout that triggered the event.</summary>
public enum WorkoutLogAction
{
    Completed,
    Reopened,
    EditRequested
}

/// <summary>
/// Custom event: broadcast when the user swipes a workout to complete, undo, or edit it.
/// The sender (WorkoutLogViewModel) has no idea who is listening; any subscriber
/// registered with the messenger receives it.
/// </summary>
public sealed record WorkoutLoggedMessage(Workout Workout, WorkoutLogAction Action);