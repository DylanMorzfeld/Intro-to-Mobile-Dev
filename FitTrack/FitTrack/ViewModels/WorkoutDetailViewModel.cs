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
    private readonly IAppNavigator _navigator;

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

    public WorkoutDetailViewModel(IWorkoutRepository workoutRepository, IAppNavigator navigator)
    {
        _workoutRepository = workoutRepository;
        _navigator = navigator;
    }

    /// <summary>
    /// Auto-invoked by .NET MAUI Shell whenever WorkoutId changes as a
    /// result of navigation (enabled by the [QueryProperty] attribute
    /// above). A value of 0 means "new workout"; anything else means
    /// "load and edit this existing workout".
    /// </summary>
    partial void OnWorkoutIdChanged(int value) => _ = LoadWorkoutAsync(value);

    // Not async void: any failure is caught here and shown on the form instead of
    // vanishing into the process-wide unhandled-exception event.
    private async Task LoadWorkoutAsync(int id)
    {
        if (id == 0)
        {
            PageTitle = "Log Workout";
            return;
        }

        try
        {
            var existing = await _workoutRepository.GetByIdAsync(id);
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load workout: {ex}");
            ValidationMessage = "Couldn't load that workout.";
        }
    }

    [ObservableProperty]
    private string? validationMessage;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (DurationMinutes <= 0)
        {
            ValidationMessage = "Duration must be greater than 0 minutes.";
            return;
        }

        if (CaloriesBurned < 0)
        {
            ValidationMessage = "Calories burned can't be negative.";
            return;
        }

        ValidationMessage = null;

        _workout.ActivityType = SelectedActivityType;
        _workout.Intensity = SelectedIntensity;
        _workout.DurationMinutes = DurationMinutes;
        _workout.CaloriesBurned = CaloriesBurned;
        _workout.DateLogged = DateLogged;
        _workout.Notes = Notes;
        _workout.IsRecurring = IsRecurring;

        await _workoutRepository.SaveAsync(_workout);
        await _navigator.GoBackAsync();
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigator.GoBackAsync();
    }
}