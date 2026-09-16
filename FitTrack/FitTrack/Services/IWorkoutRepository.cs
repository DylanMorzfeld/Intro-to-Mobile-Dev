using FitTrack.Models;

namespace FitTrack.Services.Interfaces;

/// <summary>
/// Defines the contract for anything that can store and retrieve Workouts.
///
/// ViewModels will depend on THIS interface, never on a concrete
/// implementation. That's what makes the architecture future-proof: when a
/// real backend API gets added later this semester, we just write a new
/// class (e.g. ApiWorkoutRepository) that implements this same interface
/// and flip one line of dependency-injection registration in
/// MauiProgram.cs. No ViewModel or View code needs to change at all.
/// </summary>
public interface IWorkoutRepository
{
    Task<List<Workout>> GetAllAsync();

    Task<Workout?> GetByIdAsync(int id);

    /// <summary>Inserts a new workout, or updates an existing one if its Id is already set.</summary>
    Task<int> SaveAsync(Workout workout);

    Task<int> DeleteAsync(Workout workout);
}