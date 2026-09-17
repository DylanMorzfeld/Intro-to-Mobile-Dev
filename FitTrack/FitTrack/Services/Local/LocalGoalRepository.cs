using FitTrack.Models;
using FitTrack.Services.Interfaces;
using SQLite;

namespace FitTrack.Services.Local;

/// <summary>
/// SQLite-backed implementation of IGoalRepository - same pattern as
/// LocalWorkoutRepository and LocalDietRepository, sharing the same
/// underlying fittrack.db3 file via a separate table.
/// </summary>
public class LocalGoalRepository : IGoalRepository
{
    private SQLiteAsyncConnection? _database;

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fittrack.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<FitnessGoal>();

        return _database;
    }

    public async Task<List<FitnessGoal>> GetAllAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<FitnessGoal>()
                        .OrderByDescending(g => g.StartDate)
                        .ToListAsync();
    }

    public async Task<FitnessGoal?> GetByIdAsync(int id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<FitnessGoal>()
                        .Where(g => g.Id == id)
                        .FirstOrDefaultAsync();
    }

    public async Task<int> SaveAsync(FitnessGoal goal)
    {
        var db = await GetDatabaseAsync();
        return goal.Id != 0
            ? await db.UpdateAsync(goal)
            : await db.InsertAsync(goal);
    }

    public async Task<int> DeleteAsync(FitnessGoal goal)
    {
        var db = await GetDatabaseAsync();
        return await db.DeleteAsync(goal);
    }
}