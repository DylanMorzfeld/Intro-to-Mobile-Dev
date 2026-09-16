using SQLite;

namespace FitTrack.Models;

/// <summary>
/// Type of physical activity a user can log. Extend this list as new
/// activity types become relevant (e.g. more cardio or strength variants).
/// </summary>
public enum ActivityType
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

/// <summary>
/// Perceived intensity of a workout. Used for display, and could later
/// feed into calorie estimation if the user doesn't manually enter a value.
/// </summary>
public enum IntensityLevel
{
    Low,
    Moderate,
    High
}

/// <summary>
/// Represents a single logged workout session.
///
/// This is a plain data class (a "POCO" - Plain Old CLR Object) with zero
/// MAUI or database-specific code in it. Keeping it plain means the same
/// class can be used by the UI layer, the local SQLite repository, and
/// later a remote API/DTO mapping - without any changes here.
/// </summary>
public class Workout
{
    /// <summary>
    /// Primary key. Left as 0 for a brand-new, unsaved workout;
    /// SQLite assigns a real value automatically once it's persisted.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public ActivityType ActivityType { get; set; }

    /// <summary>How long the workout lasted, in minutes.</summary>
    public int DurationMinutes { get; set; }

    public IntensityLevel Intensity { get; set; }

    /// <summary>Estimated or manually entered calories burned during the session.</summary>
    public int CaloriesBurned { get; set; }

    /// <summary>The date/time the workout occurred (not necessarily when it was logged).</summary>
    public DateTime DateLogged { get; set; } = DateTime.Now;

    /// <summary>Optional free-text notes, e.g. "felt strong today" or "left knee sore".</summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this workout should repeat automatically (e.g. every Mon/Wed/Fri).
    /// For now we just store the flag - actual reminder/notification scheduling
    /// is a good candidate for a later milestone once local notifications are wired up.
    /// </summary>
    public bool IsRecurring { get; set; }
}