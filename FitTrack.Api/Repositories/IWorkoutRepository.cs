using FitTrack.Api.Models;

namespace FitTrack.Api.Repositories;

/// <summary>
/// Storage contract for workouts. The service depends on this interface, not on
/// the JSON file, so Part 2 can add a SQL Server version without changing any
/// other layer. Every method is async and takes a CancellationToken.
/// </summary>
public interface IWorkoutRepository
{
    Task<IReadOnlyList<Workout>> GetAllAsync(CancellationToken cancellationToken);

    Task<Workout?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>Assigns the next Id, saves the workout, and returns it.</summary>
    Task<Workout> AddAsync(Workout workout, CancellationToken cancellationToken);

    /// <summary>Replaces the stored workout with the same Id. Returns false if none exists.</summary>
    Task<bool> UpdateAsync(Workout workout, CancellationToken cancellationToken);

    /// <summary>Removes the workout. Returns false if none exists.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}