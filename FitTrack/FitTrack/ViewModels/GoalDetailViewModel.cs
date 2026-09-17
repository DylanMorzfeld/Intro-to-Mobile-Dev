using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitTrack.Models;
using FitTrack.Services.Interfaces;

namespace FitTrack.ViewModels;

/// <summary>
/// Backs GoalDetailPage, used for both creating a new goal and updating
/// progress on an existing one - same shape as WorkoutDetailViewModel and
/// MealDetailViewModel.
/// </summary>
[QueryProperty(nameof(GoalId), "GoalId")]
public partial class GoalDetailViewModel : ObservableObject
{
    private readonly IGoalRepository _goalRepository;
    private FitnessGoal _goal = new();

    [ObservableProperty]
    private int goalId;

    [ObservableProperty]
    private string pageTitle = "New Goal";

    [ObservableProperty]
    private GoalType selectedGoalType = GoalType.WeightLoss;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private double startValue;

    [ObservableProperty]
    private double currentValue;

    [ObservableProperty]
    private double targetValue;

    [ObservableProperty]
    private string unit = string.Empty;

    [ObservableProperty]
    private DateTime startDate = DateTime.Now;

    [ObservableProperty]
    private DateTime targetDate = DateTime.Now.AddMonths(1);

    public List<GoalType> GoalTypes { get; } = Enum.GetValues<GoalType>().ToList();

    public GoalDetailViewModel(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    async partial void OnGoalIdChanged(int value)
    {
        if (value == 0)
        {
            PageTitle = "New Goal";
            return;
        }

        var existing = await _goalRepository.GetByIdAsync(value);
        if (existing is null)
            return;

        _goal = existing;
        PageTitle = "Update Goal";
        SelectedGoalType = existing.GoalType;
        Description = existing.Description;
        StartValue = existing.StartValue;
        CurrentValue = existing.CurrentValue;
        TargetValue = existing.TargetValue;
        Unit = existing.Unit;
        StartDate = existing.StartDate;
        TargetDate = existing.TargetDate ?? DateTime.Now.AddMonths(1);
    }

    [ObservableProperty]
    private string? validationMessage;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Description))
        {
            ValidationMessage = "Please enter a goal description.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Unit))
        {
            ValidationMessage = "Please enter a unit (e.g. lbs, miles).";
            return;
        }

        // For "decreasing" goals like weight loss, the target must be LOWER
        // than the starting point - otherwise "progress" and "completion"
        // can't be calculated meaningfully.
        bool isDecreasingGoal = SelectedGoalType == GoalType.WeightLoss;

        if (isDecreasingGoal && TargetValue >= StartValue)
        {
            ValidationMessage = "For weight loss, your target should be lower than your starting value.";
            return;
        }

        if (!isDecreasingGoal && TargetValue <= StartValue)
        {
            ValidationMessage = "Your target should be higher than your starting value.";
            return;
        }

        ValidationMessage = null;

        _goal.GoalType = SelectedGoalType;
        _goal.Description = Description;
        _goal.StartValue = StartValue;
        _goal.CurrentValue = CurrentValue;
        _goal.TargetValue = TargetValue;
        _goal.Unit = Unit;
        _goal.StartDate = StartDate;
        _goal.TargetDate = TargetDate;

        await _goalRepository.SaveAsync(_goal);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}