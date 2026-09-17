using FitTrack.Models;

namespace FitTrack.Services.Interfaces;

/// <summary>
/// Defines the contract for anything that can store and retrieve
/// FitnessGoals. Same reasoning as the other repository interfaces:
/// ViewModels depend on this abstraction, never a concrete implementation.
/// </summary>
public interface IGoalRepository
{
    Task<List<FitnessGoal>> GetAllAsync();

    Task<FitnessGoal?> GetByIdAsync(int id);

    Task<int> SaveAsync(FitnessGoal goal);

    Task<int> DeleteAsync(FitnessGoal goal);
}