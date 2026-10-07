using System.Text.Json;
using System.Text.Json.Serialization;
using FitTrack.Api.Models;

namespace FitTrack.Api.Repositories;

/// <summary>
/// Stores workouts in Data/workouts.json under the content root.
///
/// One SemaphoreSlim(1, 1) guards every public method, so only one request
/// touches the file at a time. That only works because this class is registered
/// as a singleton: the lock lives in the instance, so a second instance would
/// have its own lock and defeat the guard.
/// </summary>
public class JsonWorkoutRepository : IWorkoutRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _filePath;
    private readonly string _tempPath;

    public JsonWorkoutRepository(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);

        _filePath = Path.Combine(dataDirectory, "workouts.json");
        _tempPath = _filePath + ".tmp";
    }

    public async Task<IReadOnlyList<Workout>> GetAllAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await ReadAllAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Workout?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var workouts = await ReadAllAsync(cancellationToken);
            return workouts.FirstOrDefault(w => w.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<Workout> AddAsync(Workout workout, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var workouts = await ReadAllAsync(cancellationToken);

            // The id is assigned here, inside the guarded section, so two
            // simultaneous POSTs can never receive the same id.
            workout.Id = workouts.Count == 0 ? 1 : workouts.Max(w => w.Id) + 1;

            workouts.Add(workout);
            await WriteAllAsync(workouts, cancellationToken);
            return workout;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> UpdateAsync(Workout workout, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var workouts = await ReadAllAsync(cancellationToken);

            var index = workouts.FindIndex(w => w.Id == workout.Id);
            if (index < 0)
                return false;

            workouts[index] = workout;
            await WriteAllAsync(workouts, cancellationToken);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var workouts = await ReadAllAsync(cancellationToken);

            if (workouts.RemoveAll(w => w.Id == id) == 0)
                return false;

            await WriteAllAsync(workouts, cancellationToken);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    // --- Private helpers: only ever called from inside the guarded sections above. ---

    /// <summary>A missing or empty file is an empty list, not a crash.</summary>
    private async Task<List<Workout>> ReadAllAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
            return new List<Workout>();

        var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
            return new List<Workout>();

        return JsonSerializer.Deserialize<List<Workout>>(json, JsonOptions) ?? new List<Workout>();
    }

    /// <summary>
    /// Writes to a temp file first, then swaps it into place. If the process dies
    /// mid-write, workouts.json is still the last complete version instead of a
    /// half-written file.
    /// </summary>
    private async Task WriteAllAsync(List<Workout> workouts, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(workouts, JsonOptions);
        await File.WriteAllTextAsync(_tempPath, json, cancellationToken);
        File.Move(_tempPath, _filePath, overwrite: true);
    }
}