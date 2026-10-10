using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitTrack.Models;
using FitTrack.Services.Interfaces;

namespace FitTrack.ViewModels;

/// <summary>
/// Backs MealDetailPage, used for both logging a new meal and editing an
/// existing one - identical shape to WorkoutDetailViewModel.
/// </summary>
[QueryProperty(nameof(MealId), "MealId")]
public partial class MealDetailViewModel : ObservableObject
{
    private readonly IDietRepository _dietRepository;
    private readonly IAppNavigator _navigator;
    private Meal _meal = new();

    [ObservableProperty]
    private int mealId;

    [ObservableProperty]
    private string pageTitle = "Log Meal";

    [ObservableProperty]
    private MealType selectedMealType = MealType.Breakfast;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int calories;

    [ObservableProperty]
    private double protein;

    [ObservableProperty]
    private double carbs;

    [ObservableProperty]
    private double fat;

    [ObservableProperty]
    private DateTime dateLogged = DateTime.Now;

    public List<MealType> MealTypes { get; } = Enum.GetValues<MealType>().ToList();

    public MealDetailViewModel(IDietRepository dietRepository, IAppNavigator navigator)
    {
        _dietRepository = dietRepository;
        _navigator = navigator;
    }

    partial void OnMealIdChanged(int value) => _ = LoadMealAsync(value);

    // Not async void: any failure is caught here and shown on the form.
    private async Task LoadMealAsync(int id)
    {
        if (id == 0)
        {
            PageTitle = "Log Meal";
            return;
        }

        try
        {
            var existing = await _dietRepository.GetByIdAsync(id);
            if (existing is null)
                return;

            _meal = existing;
            PageTitle = "Edit Meal";
            SelectedMealType = existing.MealType;
            Name = existing.Name;
            Calories = existing.Calories;
            Protein = existing.Protein;
            Carbs = existing.Carbs;
            Fat = existing.Fat;
            DateLogged = existing.DateLogged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load meal: {ex}");
            ValidationMessage = "Couldn't load that meal.";
        }
    }

    [ObservableProperty]
    private string? validationMessage;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = "Please enter a food or meal name.";
            return;
        }

        if (Calories < 0)
        {
            ValidationMessage = "Calories can't be negative.";
            return;
        }

        ValidationMessage = null;

        _meal.MealType = SelectedMealType;
        _meal.Name = Name;
        _meal.Calories = Calories;
        _meal.Protein = Protein;
        _meal.Carbs = Carbs;
        _meal.Fat = Fat;
        _meal.DateLogged = DateLogged;

        await _dietRepository.SaveAsync(_meal);
        await _navigator.GoBackAsync();
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigator.GoBackAsync();
    }
}