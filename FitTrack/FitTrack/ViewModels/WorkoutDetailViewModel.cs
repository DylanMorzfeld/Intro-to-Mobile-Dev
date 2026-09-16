using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitTrack.Models;
using FitTrack.Services.Interfaces;

namespace FitTrack.ViewModels;

/// <summary>
/// Backs WorkoutDetailPage, used for both creating a new workout and
/// editing an existing one. Which mode it's in depends on whether a
/// WorkoutId was passed in via navigation.
/// </summary>
[QueryProperty(nameof(WorkoutId), "WorkoutId")]
public partial class WorkoutDetailViewModel : ObservableObject
{
    private readonly IWorkoutRepository _workoutRepository;

    // Tracks the actual Workout object being edited/created behind the
    // scenes. The individual [ObservableProperty] fields below are what
    // the View actually binds to - keeping them separate from this object
    // makes it easy to add per-field validation later without restructuring
    // any XAML bindings.
    private Workout _workout = new();

    [ObservableProperty]
    private int workoutId;

    [ObservableProperty]
    private string pageTitle = "Log Workout";

    [ObservableProperty]
    private ActivityType selectedActivityType = ActivityType.Running;

    [ObservableProperty]
    private IntensityLevel selectedIntensity = IntensityLevel.Moderate;

    [ObservableProperty]
    private int durationMinutes;

    [ObservableProperty]
    private int caloriesBurned;

    [ObservableProperty]
    private DateTime dateLogged = DateTime.Now;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private bool isRecurring;

    // Backing lists for the two Pickers in the View.
    public List<ActivityType> ActivityTypes { get; } = Enum.GetValues<ActivityType>().ToList();
    public List<IntensityLevel> IntensityLevels { get; } = Enum.GetValues<IntensityLevel>().ToList();

    public WorkoutDetailViewModel(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    /// <summary>
    /// Auto-invoked by .NET MAUI Shell whenever WorkoutId changes as a
    /// result of navigation (enabled by the [QueryProperty] attribute
    /// above). A value of 0 means "new workout"; anything else means
    /// "load and edit this existing workout".
    /// </summary>
    async partial void OnWorkoutIdChanged(int value)
    {
        if (value == 0)
        {
            PageTitle = "Log Workout";
            return;
        }

        var existing = await _workoutRepository.GetByIdAsync(value);
        if (existing is null)
            return;

        _workout = existing;
        PageTitle = "Edit Workout";
        SelectedActivityType = existing.ActivityType;
        SelectedIntensity = existing.Intensity;
        DurationMinutes = existing.DurationMinutes;
        CaloriesBurned = existing.CaloriesBurned;
        DateLogged = existing.DateLogged;
        Notes = existing.Notes;
        IsRecurring = existing.IsRecurring;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        _workout.ActivityType = SelectedActivityType;
        _workout.Intensity = SelectedIntensity;
        _workout.DurationMinutes = DurationMinutes;
        _workout.CaloriesBurned = CaloriesBurned;
        _workout.DateLogged = DateLogged;
        _workout.Notes = Notes;
        _workout.IsRecurring = IsRecurring;

        await _workoutRepository.SaveAsync(_workout);

        // Navigate back to the workout list after saving.
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}