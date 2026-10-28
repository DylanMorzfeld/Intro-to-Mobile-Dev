using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitTrack.Models;
using FitTrack.Services.Api;
using FitTrack.Services.Interfaces;

namespace FitTrack.ViewModels;

/// <summary>
/// Backs WorkoutDetailPage, used for both creating a new workout and editing an existing one.
/// Which mode it is in depends on whether a WorkoutId was passed in via navigation.
/// Saving goes to the API; the API's answer (validation errors, duplicates, 401) is shown here.
/// </summary>
[QueryProperty(nameof(WorkoutId), "WorkoutId")]
public partial class WorkoutDetailViewModel : ObservableObject
{
    private readonly IWorkoutApiClient _api;
    private readonly ITokenStore _tokens;
    private readonly IAppNavigator _navigator;

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

    [ObservableProperty]
    private string? validationMessage;

    [ObservableProperty]
    private bool isSaving;

    public List<ActivityType> ActivityTypes { get; } = Enum.GetValues<ActivityType>().ToList();
    public List<IntensityLevel> IntensityLevels { get; } = Enum.GetValues<IntensityLevel>().ToList();

    public WorkoutDetailViewModel(IWorkoutApiClient api, ITokenStore tokens, IAppNavigator navigator)
    {
        _api = api;
        _tokens = tokens;
        _navigator = navigator;
    }

    // Shell sets WorkoutId from the navigation parameter. 0 or never set = new workout.
    partial void OnWorkoutIdChanged(int value) => _ = LoadWorkoutAsync(value);

    // Not async void: any failure is caught here and shown on the form.
    private async Task LoadWorkoutAsync(int id)
    {
        if (id == 0)
        {
            PageTitle = "Log Workout";
            return;
        }

        try
        {
            var result = await _api.GetByIdAsync(id);

            if (result.IsNotFound)
            {
                // Deleted elsewhere since the list loaded: leave the form; the list refreshes on return.
                await _navigator.GoBackAsync();
                return;
            }

            if (!result.Success)
            {
                ValidationMessage = result.ErrorMessage;
                return;
            }

            var existing = result.Value!;
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

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsSaving)
            return;

        // Quick checks that need no network. The API checks everything again; its answers are shown below.
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

        try
        {
            IsSaving = true;

            ApiResult result = _workout.Id == 0
                ? await _api.CreateAsync(_workout)
                : await _api.UpdateAsync(_workout);

            if (result.Success)
            {
                await _navigator.GoBackAsync();
                return;
            }

            if (result.IsUnauthorized)
            {
                // Token expired or rejected. The form stays as it is, underneath the login page.
                await _tokens.ClearAsync();
                await _navigator.GoToAsync(nameof(Views.LoginPage),
                    new Dictionary<string, object> { { "Message", "Your session ended. Please log in to save." } });
                return;
            }

            if (result.IsNotFound)
            {
                await _navigator.GoBackAsync();
                return;
            }

            // 400 (field errors), 409 (duplicate), 429, 5xx, and no connection all land here.
            ValidationMessage = result.ErrorMessage;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save workout: {ex}");
            ValidationMessage = "Couldn't save that workout. Try again.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigator.GoBackAsync();
    }
}
