namespace FitTrack.Api.Repositories;

/// <summary>
/// Thrown by a repository when storage itself refuses a second workout of the same type on the same date
/// (the SQL unique index). The service turns it into the normal 409 outcome.
/// </summary>
public class DuplicateWorkoutException : Exception
{
    public DuplicateWorkoutException(Exception? inner = null)
        : base("A workout of this type already exists on that date.", inner)
    {
    }
}
