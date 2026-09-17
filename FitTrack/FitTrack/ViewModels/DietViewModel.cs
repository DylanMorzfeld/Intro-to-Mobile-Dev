using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public DietViewModel(IDietRepository dietRepository)
    {
        _dietRepository = dietRepository;
    }

    [RelayCommand]
    private async Task LoadMealsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var meals = await _dietRepository.GetByDateAsync(DateTime.Today);

            Meals.Clear();
            foreach (var meal in meals)
                Meals.Add(meal);

            RecalculateTotals();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load meals: {ex.Message}");
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
        await Shell.Current.GoToAsync(nameof(Views.MealDetailPage));
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

        await Shell.Current.GoToAsync(nameof(Views.MealDetailPage), navigationParameter);
    }

    [RelayCommand]
    private async Task DeleteMealAsync(Meal? meal)
    {
        if (meal is null)
            return;

        bool confirmed = await Shell.Current.DisplayAlert(
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
}