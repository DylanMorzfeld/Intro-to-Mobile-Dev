using FitTrack.Api.DTOs;
using FitTrack.Api.Models;

namespace FitTrack.Api.Services;

public interface IWorkoutService
{
    Task<IReadOnlyList<WorkoutResponse>> GetAllAsync(WorkoutType? type, CancellationToken cancellationToken);

    Task<WorkoutResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<WorkoutResult> CreateAsync(WorkoutRequest request, CancellationToken cancellationToken);

    Task<WorkoutResult> UpdateAsync(int id, WorkoutRequest request, CancellationToken cancellationToken);

    Task<ServiceOutcome> DeleteAsync(int id, CancellationToken cancellationToken);
}
