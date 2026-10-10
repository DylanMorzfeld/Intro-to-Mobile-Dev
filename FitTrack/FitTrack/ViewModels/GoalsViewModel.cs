using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitTrack.Models;
using FitTrack.Services.Interfaces;
using System.Collections.ObjectModel;

namespace FitTrack.ViewModels;

/// <summary>
/// Backs GoalsProgressPage: the list of the user's fitness goals and their
/// progress. Same MVVM shape as WorkoutLogViewModel and DietViewModel.
/// </summary>
public partial class GoalsViewModel : ObservableObject
{
    private readonly IGoalRepository _goalRepository;
    private readonly IAppNavigator _navigator;

    public ObservableCollection<FitnessGoal> Goals { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    /// <summary>Shown on the page when loading fails, so the user is not left with a silent blank list.</summary>
    [ObservableProperty]
    private string? errorMessage;

    private readonly IActivityFeed _activityFeed;

    /// <summary>Recent activity captured from the app's custom events.</summary>
    public ObservableCollection<string> RecentActivity => _activityFeed.RecentActivity;

    public GoalsViewModel(IGoalRepository goalRepository, IActivityFeed activityFeed, IAppNavigator navigator)
    {
        _goalRepository = goalRepository;
        _activityFeed = activityFeed;
        _navigator = navigator;
    }

    [RelayCommand]
    private async Task LoadGoalsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var goals = await _goalRepository.GetAllAsync();

            Goals.Clear();
            foreach (var goal in goals)
                Goals.Add(goal);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load goals: {ex}");
            ErrorMessage = "Couldn't load your goals. Pull down to try again.";
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task AddGoalAsync()
    {
        await _navigator.GoToAsync(nameof(Views.GoalDetailPage));
    }

    [RelayCommand]
    private async Task SelectGoalAsync(FitnessGoal? goal)
    {
        if (goal is null)
            return;

        var navigationParameter = new Dictionary<string, object>
        {
            { "GoalId", goal.Id }
        };

        await _navigator.GoToAsync(nameof(Views.GoalDetailPage), navigationParameter);
    }

    [RelayCommand]
    private async Task DeleteGoalAsync(FitnessGoal? goal)
    {
        if (goal is null)
            return;

        bool confirmed = await _navigator.ConfirmAsync(
            "Delete Goal",
            $"Delete the goal \"{goal.Description}\"? This can't be undone.",
            "Delete",
            "Cancel");

        if (!confirmed)
            return;

        await _goalRepository.DeleteAsync(goal);
        Goals.Remove(goal);
    }
}