using FitTrack.Api.Models;

namespace FitTrack.Api.DTOs;

/// <summary>
/// What the API returns for a workout. It includes the server-controlled
/// fields (Id, CreatedUtc) that clients can read but never set.
/// </summary>
public class WorkoutResponse
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public WorkoutType Type { get; set; }
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public IntensityLevel Intensity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; }
}