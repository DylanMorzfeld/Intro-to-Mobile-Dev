using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FitTrack.Messages;
using FitTrack.Models;
using FitTrack.Services.Interfaces;
using System.Collections.ObjectModel;

namespace FitTrack.ViewModels;

/// <summary>
/// Backs DietTrackerPage: today's logged meals plus a running nutrition
/// total for the day. Same MVVM shape as WorkoutLogViewModel - no XAML or
/// control references here, only bindable state and commands.
/// </summary>
public partial class DietViewModel : ObservableObject
{
    private readonly IDietRepository _dietRepository;

    public ObservableCollection<Meal> Meals { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    /// <summary>Shown on the page when loading fails, so the user is not left with a silent blank list.</summary>
    [ObservableProperty]
    private string? errorMessage;

    // --- Daily nutrition totals ------------------------------------
    // Recomputed from Meals every time the list changes, rather than
    // stored independently, so they can never drift out of sync with
    // what's actually logged.

    [ObservableProperty]
    private int totalCalories;

    [ObservableProperty]
    private double totalProtein;

    [ObservableProperty]
    private double totalCarbs;

    [ObservableProperty]
    private double totalFat;

    /// <summary>
    /// Simple daily calorie target for the "goals vs. actual" insight.
    /// Hardcoded for now - once the full Goals feature exists, this should
    /// pull from the user's actual saved goal instead.
    /// </summary>
    [ObservableProperty]
    private int calorieGoal = 2000;

    private readonly IMessenger _messenger;
    private readonly IAppNavigator _navigator;

    public DietViewModel(IDietRepository dietRepository, IMessenger messenger, IAppNavigator navigator)
    {
        _dietRepository = dietRepository;
        _messenger = messenger;
        _navigator = navigator;
    }

    [RelayCommand]
    private async Task LoadMealsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var meals = await _dietRepository.GetByDateAsync(DateTime.Today);

            Meals.Clear();
            foreach (var meal in meals)
                Meals.Add(meal);

            RecalculateTotals();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load meals: {ex}");
            ErrorMessage = "Couldn't load your meals. Pull down to try again.";
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task AddMealAsync()
    {
        await _navigator.GoToAsync(nameof(Views.MealDetailPage));
    }

    [RelayCommand]
    private async Task SelectMealAsync(Meal? meal)
    {
        if (meal is null)
            return;

        var navigationParameter = new Dictionary<string, object>
        {
            { "MealId", meal.Id }
        };

        await _navigator.GoToAsync(nameof(Views.MealDetailPage), navigationParameter);
    }

    [RelayCommand]
    private async Task DeleteMealAsync(Meal? meal)
    {
        if (meal is null)
            return;

        bool confirmed = await _navigator.ConfirmAsync(
            "Delete Meal",
            $"Delete \"{meal.Name}\" from your log?",
            "Delete",
            "Cancel");

        if (!confirmed)
            return;

        await _dietRepository.DeleteAsync(meal);
        Meals.Remove(meal);
        RecalculateTotals();
    }

    /// <summary>
    /// Sums nutrition values across today's logged meals. Kept as a plain
    /// private method (not a command) since it's an internal calculation,
    /// not something the View triggers directly.
    /// </summary>
    private void RecalculateTotals()
    {
        TotalCalories = Meals.Sum(m => m.Calories);
        TotalProtein = Meals.Sum(m => m.Protein);
        TotalCarbs = Meals.Sum(m => m.Carbs);
        TotalFat = Meals.Sum(m => m.Fat);
    }

    /// <summary>
    /// Expands or collapses a meal's detailed nutrition breakdown when the row is tapped.
    /// Replacing the item in the collection forces the CollectionView to re-render that
    /// row, the same pattern ToggleCompleteAsync uses on the Workout Log.
    /// </summary>
    [RelayCommand]
    private void ToggleMealDetails(Meal? meal)
    {
        if (meal is null)
            return;

        meal.IsExpanded = !meal.IsExpanded;

        _messenger.Send(new NutritionalDetailsRequestedMessage(meal, meal.IsExpanded));

        var index = Meals.IndexOf(meal);
        if (index >= 0)
            Meals[index] = meal;
    }
}