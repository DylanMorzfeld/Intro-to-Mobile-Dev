using FitTrack.Api.DTOs;

namespace FitTrack.Api.Services;

/// <summary>
/// What happened, with no HTTP codes. The service reports an outcome and the
/// controller decides which status code it maps to.
/// </summary>
public enum ServiceOutcome
{
    Success,
    NotFound,
    Duplicate
}

public sealed record WorkoutResult(ServiceOutcome Outcome, WorkoutResponse? Workout = null);
