using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FitTrack.Messages;
using FitTrack.Models;
using FitTrack.Services.Api;
using FitTrack.Services.Interfaces;

namespace FitTrack.ViewModels;

/// <summary>
/// Backs WorkoutLogPage: the list of workouts, loaded from the API.
///
/// Browsing needs no login (the API's GETs are public). Every write (add, edit, delete, complete)
/// first checks for a token; without one the user is sent to the login page. If the token is
/// rejected anyway (expired, or the API restarted with a new key), the API answers 401, the saved
/// token is cleared, and the user is sent to the login page too.
/// </summary>
public partial class WorkoutLogViewModel : ObservableObject
{
    private const string LoginMessage = "Please log in to make changes.";

    private readonly IWorkoutApiClient _api;
    private readonly ITokenStore _tokens;
    private readonly IMessenger _messenger;
    private readonly IAppNavigator _navigator;

    private CancellationTokenSource? _loadCts;

    public ObservableCollection<Workout> Workouts { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    /// <summary>Shown on the page when something fails, so the user is never left with a silent blank list.</summary>
    [ObservableProperty]
    private string? errorMessage;

    /// <summary>"Log in" or "Log out", shown in the toolbar.</summary>
    [ObservableProperty]
    private string authButtonText = "Log in";

    public WorkoutLogViewModel(IWorkoutApiClient api, ITokenStore tokens, IMessenger messenger, IAppNavigator navigator)
    {
        _api = api;
        _tokens = tokens;
        _messenger = messenger;
        _navigator = navigator;
    }

    // AllowConcurrentExecutions: a new load cancels the one still running instead of being ignored,
    // so the newest request always wins and a slow old response can never overwrite a newer list.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task LoadWorkoutsAsync()
    {
        _loadCts?.Cancel();
        var cts = new CancellationTokenSource();
        _loadCts = cts;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await RefreshAuthStateAsync();

            var result = await _api.GetAllAsync(cts.Token);
            if (cts.IsCancellationRequested)
                return;

            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            Workouts.Clear();
            foreach (var workout in result.Value!)
                Workouts.Add(workout);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Replaced by a newer load, or the page went away. Not an error.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load workouts: {ex}");
            ErrorMessage = "Couldn't load your workouts. Pull down to try again.";
        }
        finally
        {
            if (ReferenceEquals(_loadCts, cts))
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }
    }

    /// <summary>Called by the page when it disappears, so a request never outlives its screen.</summary>
    public void CancelLoad() => _loadCts?.Cancel();

    [RelayCommand]
    private async Task AddWorkoutAsync()
    {
        if (!await EnsureLoggedInAsync())
            return;

        await _navigator.GoToAsync(nameof(Views.WorkoutDetailPage));
    }

    [RelayCommand]
    private async Task SelectWorkoutAsync(Workout? workout)
    {
        if (workout is null)
            return;

        if (!await EnsureLoggedInAsync())
            return;

        _messenger.Send(new WorkoutLoggedMessage(workout, WorkoutLogAction.EditRequested));

        var navigationParameter = new Dictionary<string, object>
        {
            { "WorkoutId", workout.Id }
        };

        await _navigator.GoToAsync(nameof(Views.WorkoutDetailPage), navigationParameter);
    }

    [RelayCommand]
    private async Task DeleteWorkoutAsync(Workout? workout)
    {
        if (workout is null)
            return;

        if (!await EnsureLoggedInAsync())
            return;

        bool confirmed = await _navigator.ConfirmAsync(
            "Delete Workout",
            $"Delete this {workout.ActivityType} workout from {workout.DateLogged:MMM d}?",
            "Delete",
            "Cancel");

        if (!confirmed)
            return;

        ErrorMessage = null;
        var result = await _api.DeleteAsync(workout.Id);

        if (await HandleUnauthorizedAsync(result))
            return;

        if (result.Success || result.IsNotFound)
        {
            // 404 means it is already gone, which is what the user wanted.
            Workouts.Remove(workout);
            return;
        }

        ErrorMessage = result.ErrorMessage;
    }

    [RelayCommand]
    private async Task ToggleCompleteAsync(Workout? workout)
    {
        if (workout is null)
            return;

        if (!await EnsureLoggedInAsync())
            return;

        ErrorMessage = null;
        workout.IsCompleted = !workout.IsCompleted;
        var result = await _api.UpdateAsync(workout);

        if (!result.Success)
        {
            workout.IsCompleted = !workout.IsCompleted; // put it back: the API did not save the change

            if (await HandleUnauthorizedAsync(result))
                return;

            ErrorMessage = result.ErrorMessage;
            return;
        }

        _messenger.Send(new WorkoutLoggedMessage(
            workout,
            workout.IsCompleted ? WorkoutLogAction.Completed : WorkoutLogAction.Reopened));

        // Reload from the API so every row redraws with the saved state.
        await LoadWorkoutsAsync();
    }

    [RelayCommand]
    private async Task ToggleLoginAsync()
    {
        if (await _tokens.GetTokenAsync() is not null)
        {
            await _tokens.ClearAsync();
            AuthButtonText = "Log in";
            ErrorMessage = null;
            return;
        }

        await GoToLoginAsync("Log in to add, edit, or delete workouts.");
    }

    private async Task RefreshAuthStateAsync()
    {
        AuthButtonText = await _tokens.GetTokenAsync() is null ? "Log in" : "Log out";
    }

    /// <summary>True when a valid token exists. Otherwise opens the login page and returns false.</summary>
    private async Task<bool> EnsureLoggedInAsync()
    {
        if (await _tokens.GetTokenAsync() is not null)
            return true;

        await GoToLoginAsync(LoginMessage);
        return false;
    }

    /// <summary>The API said 401: forget the token and ask the user to log in again.</summary>
    private async Task<bool> HandleUnauthorizedAsync(ApiResult result)
    {
        if (!result.IsUnauthorized)
            return false;

        await _tokens.ClearAsync();
        AuthButtonText = "Log in";
        await GoToLoginAsync(LoginMessage);
        return true;
    }

    private Task GoToLoginAsync(string message) =>
        _navigator.GoToAsync(nameof(Views.LoginPage), new Dictionary<string, object> { { "Message", message } });
}
