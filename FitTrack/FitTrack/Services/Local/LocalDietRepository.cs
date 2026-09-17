using FitTrack.Models;
using FitTrack.Services.Interfaces;
using SQLite;

namespace FitTrack.Services.Local;

/// <summary>
/// SQLite-backed implementation of IDietRepository. Follows the exact same
/// pattern as LocalWorkoutRepository - lazy connection, CreateTableAsync
/// on first use, and straightforward CRUD against the Meal table.
/// </summary>
public class LocalDietRepository : IDietRepository
{
    private SQLiteAsyncConnection? _database;

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        // Reuses the same fittrack.db3 file as LocalWorkoutRepository -
        // SQLite supports multiple tables in one database file, so both
        // repositories can safely share it.
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fittrack.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<Meal>();

        return _database;
    }

    public async Task<List<Meal>> GetAllAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Meal>()
                        .OrderByDescending(m => m.DateLogged)
                        .ToListAsync();
    }

    public async Task<List<Meal>> GetByDateAsync(DateTime date)
    {
        var db = await GetDatabaseAsync();
        var allMeals = await db.Table<Meal>().ToListAsync();

        // Filtering by date-only (ignoring time) is done in memory here
        // since SQLite's date functions can be finicky with C# DateTime
        // formatting - fine for the data volumes a single user generates.
        return allMeals
            .Where(m => m.DateLogged.Date == date.Date)
            .OrderByDescending(m => m.DateLogged)
            .ToList();
    }

    public async Task<Meal?> GetByIdAsync(int id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Meal>()
                        .Where(m => m.Id == id)
                        .FirstOrDefaultAsync();
    }

    public async Task<int> SaveAsync(Meal meal)
    {
        var db = await GetDatabaseAsync();
        return meal.Id != 0
            ? await db.UpdateAsync(meal)
            : await db.InsertAsync(meal);
    }

    public async Task<int> DeleteAsync(Meal meal)
    {
        var db = await GetDatabaseAsync();
        return await db.DeleteAsync(meal);
    }
}