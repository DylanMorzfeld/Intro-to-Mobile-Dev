using FitTrack.Models;
using FitTrack.Services.Api;

namespace FitTrack.Services.Interfaces;

/// <summary>
/// Every workout read and write goes through the API with this interface. The app never opens
/// a SQL connection itself; the API is the only thing that talks to SQL Server.
/// </summary>
public interface IWorkoutApiClient
{
    Task<ApiResult<List<Workout>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<Workout>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResult<Workout>> CreateAsync(Workout workout, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateAsync(Workout workout, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
