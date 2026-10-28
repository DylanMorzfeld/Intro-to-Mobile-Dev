using System.Net.Http.Json;
using FitTrack.Models;
using FitTrack.Services.Interfaces;

namespace FitTrack.Services.Api;

/// <summary>
/// Typed HttpClient for the workouts endpoints. BaseAddress, the 15-second timeout, and the
/// bearer-token handler are set up where it is registered (MauiProgram).
/// </summary>
public class WorkoutApiClient : IWorkoutApiClient
{
    private readonly HttpClient _http;

    public WorkoutApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResult<List<Workout>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.GetAsync("api/workouts", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<Workout>>.Fail(response.StatusCode, await ApiJson.ReadProblemAsync(response, cancellationToken));

            var dtos = await response.Content.ReadFromJsonAsync<List<WorkoutDto>>(ApiJson.Options, cancellationToken) ?? new();
            return ApiResult<List<Workout>>.Ok(response.StatusCode, dtos.Select(ToModel).ToList());
        }
        catch (Exception ex) when (ApiResult.IsNetworkFailure(ex, cancellationToken))
        {
            return ApiResult<List<Workout>>.NetworkFailure();
        }
    }

    public async Task<ApiResult<Workout>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.GetAsync($"api/workouts/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ApiResult<Workout>.Fail(response.StatusCode, await ApiJson.ReadProblemAsync(response, cancellationToken));

            var dto = await response.Content.ReadFromJsonAsync<WorkoutDto>(ApiJson.Options, cancellationToken);
            return dto is null
                ? ApiResult<Workout>.Fail(System.Net.HttpStatusCode.InternalServerError, null)
                : ApiResult<Workout>.Ok(response.StatusCode, ToModel(dto));
        }
        catch (Exception ex) when (ApiResult.IsNetworkFailure(ex, cancellationToken))
        {
            return ApiResult<Workout>.NetworkFailure();
        }
    }

    public async Task<ApiResult<Workout>> CreateAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync("api/workouts", ToRequest(workout), ApiJson.Options, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ApiResult<Workout>.Fail(response.StatusCode, await ApiJson.ReadProblemAsync(response, cancellationToken));

            var dto = await response.Content.ReadFromJsonAsync<WorkoutDto>(ApiJson.Options, cancellationToken);
            return dto is null
                ? ApiResult<Workout>.Fail(System.Net.HttpStatusCode.InternalServerError, null)
                : ApiResult<Workout>.Ok(response.StatusCode, ToModel(dto));
        }
        catch (Exception ex) when (ApiResult.IsNetworkFailure(ex, cancellationToken))
        {
            return ApiResult<Workout>.NetworkFailure();
        }
    }

    public async Task<ApiResult> UpdateAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.PutAsJsonAsync($"api/workouts/{workout.Id}", ToRequest(workout), ApiJson.Options, cancellationToken);
            return response.IsSuccessStatusCode
                ? ApiResult.Ok(response.StatusCode)
                : ApiResult.Fail(response.StatusCode, await ApiJson.ReadProblemAsync(response, cancellationToken));
        }
        catch (Exception ex) when (ApiResult.IsNetworkFailure(ex, cancellationToken))
        {
            return ApiResult.NetworkFailure();
        }
    }

    public async Task<ApiResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.DeleteAsync($"api/workouts/{id}", cancellationToken);
            return response.IsSuccessStatusCode
                ? ApiResult.Ok(response.StatusCode)
                : ApiResult.Fail(response.StatusCode, await ApiJson.ReadProblemAsync(response, cancellationToken));
        }
        catch (Exception ex) when (ApiResult.IsNetworkFailure(ex, cancellationToken))
        {
            return ApiResult.NetworkFailure();
        }
    }

    // The app's Workout and the API's WorkoutDto differ in two names: DateLogged/Date (the API stores
    // a date only) and ActivityType/Type.
    private static Workout ToModel(WorkoutDto dto) => new()
    {
        Id = dto.Id,
        ActivityType = dto.Type,
        DurationMinutes = dto.DurationMinutes,
        Intensity = dto.Intensity,
        CaloriesBurned = dto.CaloriesBurned,
        DateLogged = dto.Date.ToDateTime(TimeOnly.MinValue),
        Notes = dto.Notes,
        IsRecurring = dto.IsRecurring,
        IsCompleted = dto.IsCompleted
    };

    private static WorkoutRequestDto ToRequest(Workout workout) => new()
    {
        Date = DateOnly.FromDateTime(workout.DateLogged),
        Type = workout.ActivityType,
        DurationMinutes = workout.DurationMinutes,
        CaloriesBurned = workout.CaloriesBurned,
        Intensity = workout.Intensity,
        Notes = workout.Notes,
        IsCompleted = workout.IsCompleted,
        IsRecurring = workout.IsRecurring
    };
}
