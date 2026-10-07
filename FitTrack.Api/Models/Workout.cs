namespace FitTrack.Api.Models;

/// <summary>Kinds of activity a workout can be.</summary>
public enum WorkoutType
{
    Running,
    Walking,
    Cycling,
    WeightLifting,
    Swimming,
    Yoga,
    HIIT,
    Other
}

/// <summary>How hard the workout felt.</summary>
public enum IntensityLevel
{
    Low,
    Moderate,
    High
}

/// <summary>
/// The stored shape of a workout. This is the internal Model: the controller
/// never accepts or returns it directly (separate request/response DTOs
/// handle that). Id and CreatedUtc are server-controlled, so clients can never
/// set them, which is what the "over-posting is ignored" Postman test checks.
/// </summary>
public class Workout
{
    /// <summary>Assigned by the repository inside its locked section.</summary>
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public WorkoutType Type { get; set; }

    public int DurationMinutes { get; set; }

    public int CaloriesBurned { get; set; }

    public IntensityLevel Intensity { get; set; }

    public string? Notes { get; set; }

    /// <summary>Set by the service when the workout is created; never changes after that.</summary>
    public DateTime CreatedUtc { get; set; }
}