using FitTrack.Models;
using FitTrack.Services.Interfaces;
using SQLite;

namespace FitTrack.Services.Local;

/// <summary>
/// SQLite-backed implementation of IWorkoutRepository.
///
/// Because ViewModels only ever talk to IWorkoutRepository (see that
/// interface for the reasoning), this class can be swapped out later -
/// e.g. for an ApiWorkoutRepository that calls a REST backend - without
/// touching any UI or ViewModel code.
/// </summary>
public class LocalWorkoutRepository : IWorkoutRepository
{
    private SQLiteAsyncConnection? _database;

    /// <summary>
    /// Lazily opens (and creates, if needed) the local SQLite database file.
    /// Doing this lazily avoids slowing down app startup before it's actually needed.
    /// </summary>
    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fittrack.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<Workout>();

        return _database;
    }

    public async Task<List<Workout>> GetAllAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Workout>()
                        .OrderByDescending(w => w.DateLogged)
                        .ToListAsync();
    }

    public async Task<Workout?> GetByIdAsync(int id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Workout>()
                        .Where(w => w.Id == id)
                        .FirstOrDefaultAsync();
    }

    public async Task<int> SaveAsync(Workout workout)
    {
        var db = await GetDatabaseAsync();

        // An Id of 0 means this workout hasn't been saved yet.
        if (workout.Id != 0)
            return await db.UpdateAsync(workout);

        return await db.InsertAsync(workout);
    }

    public async Task<int> DeleteAsync(Workout workout)
    {
        var db = await GetDatabaseAsync();
        return await db.DeleteAsync(workout);
    }
}