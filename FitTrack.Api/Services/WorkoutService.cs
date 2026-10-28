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
    // One critical section for every create and update. The duplicate check and the write must
    // happen together: without this, two simultaneous identical POSTs can both pass the check
    // before either one is saved. static because the service itself is created per request.
    private static readonly SemaphoreSlim WriteLock = new(1, 1);

    private readonly IWorkoutRepository _repository;
    private readonly ILogger<WorkoutService> _logger;

    public WorkoutService(IWorkoutRepository repository, ILogger<WorkoutService> logger)
    {
        _repository = repository;
        _logger = logger;
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
        await WriteLock.WaitAsync(cancellationToken);
        try
        {
            return await CreateLockedAsync(request, cancellationToken);
        }
        finally
        {
            WriteLock.Release();
        }
    }

    private async Task<WorkoutResult> CreateLockedAsync(WorkoutRequest request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetAllAsync(cancellationToken);
        if (IsDuplicate(existing, request, excludeId: null))
        {
            _logger.LogWarning("Create rejected: a {Type} workout already exists on {Date}", request.Type, request.Date);
            return new WorkoutResult(ServiceOutcome.Duplicate);
        }

        // Id comes from the repository; CreatedUtc is set here. The client can set neither.
        var workout = new Workout
        {
            Date = request.Date!.Value,
            Type = request.Type!.Value,
            DurationMinutes = request.DurationMinutes!.Value,
            CaloriesBurned = request.CaloriesBurned!.Value,
            Intensity = request.Intensity!.Value,
            Notes = request.Notes,
            IsCompleted = request.IsCompleted,
            IsRecurring = request.IsRecurring,
            CreatedUtc = DateTime.UtcNow
        };

        Workout saved;
        try
        {
            saved = await _repository.AddAsync(workout, cancellationToken);
        }
        catch (DuplicateWorkoutException)
        {
            _logger.LogWarning("Create rejected by the database: a {Type} workout already exists on {Date}", request.Type, request.Date);
            return new WorkoutResult(ServiceOutcome.Duplicate);
        }

        _logger.LogInformation("Created workout {WorkoutId}: {Type} on {Date}", saved.Id, saved.Type, saved.Date);
        return new WorkoutResult(ServiceOutcome.Success, ToResponse(saved));
    }

    public async Task<WorkoutResult> UpdateAsync(int id, WorkoutRequest request, CancellationToken cancellationToken)
    {
        await WriteLock.WaitAsync(cancellationToken);
        try
        {
            return await UpdateLockedAsync(id, request, cancellationToken);
        }
        finally
        {
            WriteLock.Release();
        }
    }

    private async Task<WorkoutResult> UpdateLockedAsync(int id, WorkoutRequest request, CancellationToken cancellationToken)
    {
        var current = await _repository.GetByIdAsync(id, cancellationToken);
        if (current is null)
        {
            _logger.LogWarning("Update failed: workout {WorkoutId} was not found", id);
            return new WorkoutResult(ServiceOutcome.NotFound);
        }

        // excludeId: the workout being edited must not count as its own duplicate.
        var all = await _repository.GetAllAsync(cancellationToken);
        if (IsDuplicate(all, request, excludeId: id))
        {
            _logger.LogWarning("Update rejected: another {Type} workout already exists on {Date}", request.Type, request.Date);
            return new WorkoutResult(ServiceOutcome.Duplicate);
        }

        var updated = new Workout
        {
            Id = current.Id,                 // keep the server-controlled fields
            CreatedUtc = current.CreatedUtc,
            Date = request.Date!.Value,
            Type = request.Type!.Value,
            DurationMinutes = request.DurationMinutes!.Value,
            CaloriesBurned = request.CaloriesBurned!.Value,
            Intensity = request.Intensity!.Value,
            Notes = request.Notes,
            IsCompleted = request.IsCompleted,
            IsRecurring = request.IsRecurring
        };

        try
        {
            if (!await _repository.UpdateAsync(updated, cancellationToken))
                return new WorkoutResult(ServiceOutcome.NotFound);
        }
        catch (DuplicateWorkoutException)
        {
            _logger.LogWarning("Update rejected by the database: another {Type} workout already exists on {Date}", request.Type, request.Date);
            return new WorkoutResult(ServiceOutcome.Duplicate);
        }

        _logger.LogInformation("Updated workout {WorkoutId}: {Type} on {Date}", updated.Id, updated.Type, updated.Date);
        return new WorkoutResult(ServiceOutcome.Success, ToResponse(updated));
    }

    public async Task<ServiceOutcome> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        if (!await _repository.DeleteAsync(id, cancellationToken))
        {
            _logger.LogWarning("Delete failed: workout {WorkoutId} was not found", id);
            return ServiceOutcome.NotFound;
        }

        _logger.LogInformation("Deleted workout {WorkoutId}", id);
        return ServiceOutcome.Success;
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
        IsCompleted = w.IsCompleted,
        IsRecurring = w.IsRecurring,
        CreatedUtc = w.CreatedUtc
    };
}
