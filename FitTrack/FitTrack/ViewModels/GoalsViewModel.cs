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

    public ObservableCollection<FitnessGoal> Goals { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    public GoalsViewModel(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    [RelayCommand]
    private async Task LoadGoalsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var goals = await _goalRepository.GetAllAsync();

            Goals.Clear();
            foreach (var goal in goals)
                Goals.Add(goal);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load goals: {ex.Message}");
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
        await Shell.Current.GoToAsync(nameof(Views.GoalDetailPage));
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

        await Shell.Current.GoToAsync(nameof(Views.GoalDetailPage), navigationParameter);
    }

    [RelayCommand]
    private async Task DeleteGoalAsync(FitnessGoal? goal)
    {
        if (goal is null)
            return;

        await _goalRepository.DeleteAsync(goal);
        Goals.Remove(goal);
    }
}