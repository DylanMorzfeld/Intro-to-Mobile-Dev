using FitTrack.Api.DTOs;
using FitTrack.Api.Models;
using FitTrack.Api.Repositories;

namespace FitTrack.Api.Services;

/// <summary>
/// Business rules live here, not in the controller or the repository.
/// Rule: only one workout of the same type is allowed per day.
/// </summary>
public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _repository;

    public WorkoutService(IWorkoutRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<WorkoutResponse>> GetAllAsync(WorkoutType? type, CancellationToken cancellationToken)
    {
        var workouts = await _repository.GetAllAsync(cancellationToken);

        return workouts
            .Where(w => type is null || w.Type == type)
            .OrderByDescending(w => w.Date)
            .ThenByDescending(w => w.Id)
            .Select(ToResponse)
            .ToList();
    }

    public async Task<WorkoutResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var workout = await _repository.GetByIdAsync(id, cancellationToken);
        return workout is null ? null : ToResponse(workout);
    }

    public async Task<WorkoutResult> CreateAsync(WorkoutRequest request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetAllAsync(cancellationToken);
        if (IsDuplicate(existing, request, excludeId: null))
            return new WorkoutResult(ServiceOutcome.Duplicate);

        // Id comes from the repository; CreatedUtc is set here. The client can set neither.
        var workout = new Workout
        {
            Date = request.Date!.Value,
            Type = request.Type!.Value,
            DurationMinutes = request.DurationMinutes!.Value,
            CaloriesBurned = request.CaloriesBurned!.Value,
            Intensity = request.Intensity!.Value,
            Notes = request.Notes,
            CreatedUtc = DateTime.UtcNow
        };

        var saved = await _repository.AddAsync(workout, cancellationToken);
        return new WorkoutResult(ServiceOutcome.Success, ToResponse(saved));
    }

    public async Task<WorkoutResult> UpdateAsync(int id, WorkoutRequest request, CancellationToken cancellationToken)
    {
        var current = await _repository.GetByIdAsync(id, cancellationToken);
        if (current is null)
            return new WorkoutResult(ServiceOutcome.NotFound);

        // excludeId: the workout being edited must not count as its own duplicate.
        var all = await _repository.GetAllAsync(cancellationToken);
        if (IsDuplicate(all, request, excludeId: id))
            return new WorkoutResult(ServiceOutcome.Duplicate);

        var updated = new Workout
        {
            Id = current.Id,                 // keep the server-controlled fields
            CreatedUtc = current.CreatedUtc,
            Date = request.Date!.Value,
            Type = request.Type!.Value,
            DurationMinutes = request.DurationMinutes!.Value,
            CaloriesBurned = request.CaloriesBurned!.Value,
            Intensity = request.Intensity!.Value,
            Notes = request.Notes
        };

        if (!await _repository.UpdateAsync(updated, cancellationToken))
            return new WorkoutResult(ServiceOutcome.NotFound);

        return new WorkoutResult(ServiceOutcome.Success, ToResponse(updated));
    }

    public async Task<ServiceOutcome> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return await _repository.DeleteAsync(id, cancellationToken)
            ? ServiceOutcome.Success
            : ServiceOutcome.NotFound;
    }

    private static bool IsDuplicate(IReadOnlyList<Workout> existing, WorkoutRequest request, int? excludeId)
    {
        return existing.Any(w =>
            w.Id != excludeId &&
            w.Date == request.Date!.Value &&
            w.Type == request.Type!.Value);
    }

    private static WorkoutResponse ToResponse(Workout w) => new()
    {
        Id = w.Id,
        Date = w.Date,
        Type = w.Type,
        DurationMinutes = w.DurationMinutes,
        CaloriesBurned = w.CaloriesBurned,
        Intensity = w.Intensity,
        Notes = w.Notes,
        CreatedUtc = w.CreatedUtc
    };
}
