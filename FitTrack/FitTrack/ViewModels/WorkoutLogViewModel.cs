using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitTrack.Models;
using FitTrack.Services.Interfaces;
using FitTrack.Models;
using FitTrack.Services.Interfaces;
using System.Collections.ObjectModel;

namespace FitTrack.ViewModels;

/// <summary>
/// Backs WorkoutLogPage: the main list of logged workouts.
///
/// This class has ZERO references to XAML, Pages, or UI controls - that's
/// the separation of concerns MVVM is built on. It exposes data via
/// bindable properties and behavior via ICommand, and the View binds to
/// both declaratively in XAML rather than the ViewModel reaching into the UI.
/// </summary>
public partial class WorkoutLogViewModel : ObservableObject
{
    private readonly IWorkoutRepository _workoutRepository;

    /// <summary>
    /// The list shown in the CollectionView. ObservableCollection
    /// automatically notifies the UI whenever items are added or removed,
    /// so the list on screen updates itself without any manual refresh code.
    /// </summary>
    public ObservableCollection<Workout> Workouts { get; } = new();

    // [ObservableProperty] is a source generator from CommunityToolkit.Mvvm.
    // Behind the scenes it generates a public "IsBusy" property plus all the
    // INotifyPropertyChanged boilerplate, from this one private field.
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    // Constructor injection: MauiProgram.cs hands this class a shared
    // IWorkoutRepository automatically. This ViewModel never has to know
    // (or care) whether that repository talks to SQLite or a remote API.
    public WorkoutLogViewModel(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    /// <summary>
    /// [RelayCommand] generates a public LoadWorkoutsCommand (ICommand).
    /// The View binds to it - e.g. a RefreshView's Command, or the page
    /// calling it directly from OnAppearing.
    /// </summary>
    [RelayCommand]
    private async Task LoadWorkoutsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var workouts = await _workoutRepository.GetAllAsync();

            Workouts.Clear();
            foreach (var workout in workouts)
                Workouts.Add(workout);
        }
        catch (Exception ex)
        {
            // TODO (later milestone): surface this via a dialog/toast
            // service instead of just writing to the debug console.
            System.Diagnostics.Debug.WriteLine($"Failed to load workouts: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Navigates to a blank WorkoutDetailPage so the user can log a new
    /// workout. No "WorkoutId" query parameter is passed, which tells the
    /// detail ViewModel this is a new entry rather than an edit.
    /// </summary>
    [RelayCommand]
    private async Task AddWorkoutAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.WorkoutDetailPage));
    }

    /// <summary>
    /// Navigates to the detail page for an existing workout, passing its Id
    /// as a query parameter so the detail ViewModel can load it for editing.
    /// </summary>
    [RelayCommand]
    private async Task SelectWorkoutAsync(Workout? workout)
    {
        if (workout is null)
            return;

        var navigationParameter = new Dictionary<string, object>
    {
        { "WorkoutId", workout.Id }
    };

        await Shell.Current.GoToAsync(nameof(Views.WorkoutDetailPage), navigationParameter);
    }

    /// <summary>
    /// Deletes a workout directly from the list (e.g. via a swipe action)
    /// without navigating away, for a fast, low-friction interaction.
    /// </summary>
    [RelayCommand]
    private async Task DeleteWorkoutAsync(Workout? workout)
    {
        if (workout is null)
            return;

        await _workoutRepository.DeleteAsync(workout);
        Workouts.Remove(workout);
    }
}