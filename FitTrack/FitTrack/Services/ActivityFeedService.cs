using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using FitTrack.Messages;

namespace FitTrack.Services;

/// <summary>
/// Subscribes to the app's custom events and keeps a short, in-memory list of
/// recent activity for display. Registered as a singleton and created at app
/// startup (see MauiProgram) so it hears events from the moment the app launches,
/// not only after a particular page has been opened.
/// </summary>
public class ActivityFeedService :
    IRecipient<WorkoutLoggedMessage>,
    IRecipient<NutritionalDetailsRequestedMessage>
{
    private const int MaxItems = 5;

    public ObservableCollection<string> RecentActivity { get; } = new();

    public ActivityFeedService(IMessenger messenger)
    {
        // RegisterAll wires up every IRecipient<T> interface this class implements.
        messenger.RegisterAll(this);
    }

    public void Receive(WorkoutLoggedMessage message)
    {
        var workout = message.Workout;

        var text = message.Action switch
        {
            WorkoutLogAction.Completed => $"✓ Completed {workout.ActivityType} ({workout.DurationMinutes} min)",
            WorkoutLogAction.Reopened => $"Reopened {workout.ActivityType} workout",
            WorkoutLogAction.EditRequested => $"Editing {workout.ActivityType} workout",
            _ => $"{workout.ActivityType} workout updated"
        };

        AddEntry(text);
    }

    public void Receive(NutritionalDetailsRequestedMessage message)
    {
        // Only log expansions; collapsing isn't interesting activity.
        if (message.IsExpanded)
            AddEntry($"Viewed nutrition for {message.Meal.Name} ({message.Meal.Calories} cal)");
    }

    private void AddEntry(string text)
    {
        RecentActivity.Insert(0, $"{DateTime.Now:h:mm tt}  {text}");

        while (RecentActivity.Count > MaxItems)
            RecentActivity.RemoveAt(RecentActivity.Count - 1);
    }
}