using FitTrack.Models;

namespace FitTrack.Messages;

/// <summary>
/// Custom event: broadcast when the user taps a meal to expand or collapse its
/// nutrition detail on the Diet Tracker.
/// </summary>
public sealed record NutritionalDetailsRequestedMessage(Meal Meal, bool IsExpanded);