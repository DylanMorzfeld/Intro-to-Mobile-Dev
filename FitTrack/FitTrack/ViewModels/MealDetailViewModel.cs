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

    public MealDetailViewModel(IDietRepository dietRepository)
    {
        _dietRepository = dietRepository;
    }

    async partial void OnMealIdChanged(int value)
    {
        if (value == 0)
        {
            PageTitle = "Log Meal";
            return;
        }

        var existing = await _dietRepository.GetByIdAsync(value);
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

    [RelayCommand]
    private async Task SaveAsync()
    {
        _meal.MealType = SelectedMealType;
        _meal.Name = Name;
        _meal.Calories = Calories;
        _meal.Protein = Protein;
        _meal.Carbs = Carbs;
        _meal.Fat = Fat;
        _meal.DateLogged = DateLogged;

        await _dietRepository.SaveAsync(_meal);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}