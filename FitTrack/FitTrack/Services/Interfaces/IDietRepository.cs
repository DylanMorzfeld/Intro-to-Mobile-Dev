using FitTrack.Models;

namespace FitTrack.Services.Interfaces;

/// <summary>
/// Defines the contract for anything that can store and retrieve Meals.
/// ViewModels depend on this interface, not on a concrete implementation -
/// same reasoning as IWorkoutRepository: swap in a real API later without
/// touching any ViewModel or View code.
/// </summary>
public interface IDietRepository
{
    Task<List<Meal>> GetAllAsync();

    /// <summary>Gets only the meals logged on a specific calendar day, ignoring time-of-day.</summary>
    Task<List<Meal>> GetByDateAsync(DateTime date);

    Task<Meal?> GetByIdAsync(int id);

    Task<int> SaveAsync(Meal meal);

    Task<int> DeleteAsync(Meal meal);
}