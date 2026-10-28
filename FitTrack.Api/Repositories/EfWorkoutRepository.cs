using FitTrack.Api.Data;
using FitTrack.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Api.Repositories;

/// <summary>
/// SQL Server version of IWorkoutRepository (EF Core). The service and controller
/// cannot tell it apart from JsonWorkoutRepository. Registered as scoped because
/// the DbContext is scoped; SQL Server handles concurrent access, so no lock is needed here.
/// </summary>
public class EfWorkoutRepository : IWorkoutRepository
{
    private readonly WorkoutDbContext _db;

    public EfWorkoutRepository(WorkoutDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Workout>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _db.Workouts.AsNoTracking().OrderBy(w => w.Id).ToListAsync(cancellationToken);
    }

    public async Task<Workout?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Workouts.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workout> AddAsync(Workout workout, CancellationToken cancellationToken)
    {
        workout.Id = 0; // SQL Server assigns the id (identity column)
        _db.Workouts.Add(workout);
        await SaveAsync(cancellationToken);
        return workout;
    }

    public async Task<bool> UpdateAsync(Workout workout, CancellationToken cancellationToken)
    {
        var existing = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workout.Id, cancellationToken);
        if (existing is null)
            return false;

        _db.Entry(existing).CurrentValues.SetValues(workout);
        await SaveAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var existing = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        if (existing is null)
            return false;

        _db.Workouts.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // 2601 = duplicate key in a unique index, 2627 = unique constraint violation.
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new DuplicateWorkoutException(ex);
        }
    }
}
