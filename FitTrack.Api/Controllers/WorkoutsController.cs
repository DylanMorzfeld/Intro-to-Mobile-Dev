using FitTrack.Api.DTOs;
using FitTrack.Api.Models;
using FitTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

/// <summary>
/// Thin by design: call the service once, then map the outcome to a status code.
/// No file access and no business rules here.
/// </summary>
[ApiController]
[Route("api/workouts")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _service;

    public WorkoutsController(IWorkoutService service)
    {
        _service = service;
    }

    /// <summary>List workouts. Optional filter: ?type=Running</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WorkoutResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] WorkoutType? type, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(type, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var workout = await _service.GetByIdAsync(id, cancellationToken);
        if (workout is null)
            return NotFound();

        return Ok(workout);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(WorkoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);

        return result.Outcome switch
        {
            // CreatedAtAction sets the Location header to the new workout's URL.
            ServiceOutcome.Success => CreatedAtAction(nameof(GetById), new { id = result.Workout!.Id }, result.Workout),
            ServiceOutcome.Duplicate => DuplicateProblem(),
            _ => throw new InvalidOperationException($"Unexpected outcome: {result.Outcome}")
        };
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, WorkoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);

        return result.Outcome switch
        {
            ServiceOutcome.Success => NoContent(),
            ServiceOutcome.NotFound => NotFound(),
            ServiceOutcome.Duplicate => DuplicateProblem(),
            _ => throw new InvalidOperationException($"Unexpected outcome: {result.Outcome}")
        };
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var outcome = await _service.DeleteAsync(id, cancellationToken);

        return outcome switch
        {
            ServiceOutcome.Success => NoContent(),
            ServiceOutcome.NotFound => NotFound(),
            _ => throw new InvalidOperationException($"Unexpected outcome: {outcome}")
        };
    }

    private ObjectResult DuplicateProblem() => Problem(
        title: "Duplicate workout",
        detail: "A workout of this type already exists on that date.",
        statusCode: StatusCodes.Status409Conflict);
}
