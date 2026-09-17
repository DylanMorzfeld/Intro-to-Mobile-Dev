using SQLite;

namespace FitTrack.Models;

/// <summary>
/// Type of meal or snack being logged. Helps organize a day's food intake
/// into recognizable groupings for display and future insights.
/// </summary>
public enum MealType
{
    Breakfast,
    Lunch,
    Dinner,
    Snack
}

/// <summary>
/// Represents a single logged food item or meal entry.
///
/// Like Workout, this is a plain data class with no MAUI or persistence
/// code in it - the same object flows through the repository, the
/// ViewModel, and the View bindings unchanged.
/// </summary>
public class Meal
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public MealType MealType { get; set; }

    /// <summary>Free-text name of the food/meal, e.g. "Grilled chicken salad".</summary>
    public string Name { get; set; } = string.Empty;

    public int Calories { get; set; }

    /// <summary>Grams of protein.</summary>
    public double Protein { get; set; }

    /// <summary>Grams of carbohydrates.</summary>
    public double Carbs { get; set; }

    /// <summary>Grams of fat.</summary>
    public double Fat { get; set; }

    /// <summary>The date/time this meal was logged.</summary>
    public DateTime DateLogged { get; set; } = DateTime.Now;
}