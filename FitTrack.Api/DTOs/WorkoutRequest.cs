using System.ComponentModel.DataAnnotations;
using FitTrack.Api.Models;

namespace FitTrack.Api.DTOs;

/// <summary>
/// What a client sends to create or replace a workout.
///
/// There is deliberately no Id or CreatedUtc here. Those are server-controlled,
/// so if a client includes them in the JSON, they are ignored (over-posting
/// protection). The Postman POST test checks exactly that.
///
/// Date, Type, Intensity, and CaloriesBurned are nullable on purpose. A missing
/// value then shows up as null and fails [Required], instead of silently
/// becoming 0 or the first enum value.
/// </summary>
public class WorkoutRequest
{
    [Required(ErrorMessage = "A workout date is required.")]
    public DateOnly? Date { get; set; }

    [Required(ErrorMessage = "A workout type is required.")]
    [EnumDataType(typeof(WorkoutType), ErrorMessage = "Type must be one of: Running, Walking, Cycling, WeightLifting, Swimming, Yoga, HIIT, Other.")]
    public WorkoutType? Type { get; set; }

    [Required(ErrorMessage = "Duration in minutes is required.")]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
    public int? DurationMinutes { get; set; }

    [Required(ErrorMessage = "Calories burned is required.")]
    [Range(0, 10000, ErrorMessage = "Calories burned must be between 0 and 10000.")]
    public int? CaloriesBurned { get; set; }

    [Required(ErrorMessage = "Intensity is required.")]
    [EnumDataType(typeof(IntensityLevel), ErrorMessage = "Intensity must be Low, Moderate, or High.")]
    public IntensityLevel? Intensity { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters.")]
    public string? Notes { get; set; }
}