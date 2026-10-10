using System.Collections.ObjectModel;

namespace FitTrack.Services.Interfaces;

/// <summary>What the Goals page needs from the activity feed.</summary>
public interface IActivityFeed
{
    ObservableCollection<string> RecentActivity { get; }
}
