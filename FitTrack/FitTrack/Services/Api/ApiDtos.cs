using FitTrack.Models;

namespace FitTrack.Services.Api;

public record LoginRequestDto(string Username, string Password);

public record LoginResponseDto(string AccessToken, string TokenType, int ExpiresIn);

/// <summary>What the app sends to create or replace a workout. Matches the API's WorkoutRequest.</summary>
public class WorkoutRequestDto
{
    public DateOnly Date { get; set; }
    public ActivityType Type { get; set; }
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public IntensityLevel Intensity { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsRecurring { get; set; }
}

/// <summary>What the API returns for a workout. Matches the API's WorkoutResponse.</summary>
public class WorkoutDto : WorkoutRequestDto
{
    public int Id { get; set; }
    public DateTime CreatedUtc { get; set; }
}
