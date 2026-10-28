using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitTrack.Services.Api;

/// <summary>JSON settings shared by every API call: camelCase names, enums as text ("Running").</summary>
public static class ApiJson
{
    public static readonly JsonSerializerOptions Options = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    /// <summary>Reads the API's problem-details body. Returns null for an empty or unreadable body (a 401 has none).</summary>
    public static async Task<ProblemDetailsDto?> ReadProblemAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            if (response.Content.Headers.ContentLength == 0)
                return null;

            return await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(Options, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }
}

/// <summary>The API's error body (RFC 7807 problem details). Errors holds the per-field messages of a 400.</summary>
public class ProblemDetailsDto
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public int? Status { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// What an API call returns. 4xx and 5xx answers are data here, not exceptions, so a view model
/// can decide what to show. StatusCode 0 means no answer arrived (API off, no network, timeout).
/// </summary>
public class ApiResult
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public ProblemDetailsDto? Problem { get; init; }

    public bool IsNetworkError => StatusCode == 0 && !Success;
    public bool IsValidationError => StatusCode == 400;
    public bool IsUnauthorized => StatusCode == 401;
    public bool IsNotFound => StatusCode == 404;
    public bool IsConflict => StatusCode == 409;

    /// <summary>A message that is safe and useful to show the user.</summary>
    public string ErrorMessage
    {
        get
        {
            if (Success)
                return string.Empty;

            switch (StatusCode)
            {
                case 0:
                    return "Can't reach the API. Is it running?";
                case 400:
                    if (Problem?.Errors is { Count: > 0 } errors)
                        return string.Join(Environment.NewLine, errors.Values.SelectMany(messages => messages).Distinct());
                    return "Fix the values marked above and try again.";
                case 401:
                    return "Please log in to make changes.";
                case 404:
                    return "That workout no longer exists.";
                case 409:
                    return Problem?.Detail ?? "A workout of that type already exists on that date.";
                case 429:
                    return "Too many attempts. Wait a minute and try again.";
                case >= 500:
                    return "The API had a problem. Try again later.";
                default:
                    return Problem?.Detail ?? $"Unexpected response ({StatusCode}).";
            }
        }
    }

    public static ApiResult Ok(HttpStatusCode status) => new() { Success = true, StatusCode = (int)status };

    public static ApiResult Fail(HttpStatusCode status, ProblemDetailsDto? problem) =>
        new() { StatusCode = (int)status, Problem = problem };

    public static ApiResult NetworkFailure() => new() { StatusCode = 0 };

    /// <summary>
    /// True when the API simply could not be reached. A caller-requested cancel is NOT a network
    /// failure, so it keeps propagating and the view model can ignore it.
    /// </summary>
    public static bool IsNetworkFailure(Exception ex, CancellationToken cancellationToken) =>
        ex is HttpRequestException
        || ex is IOException
        || (ex is OperationCanceledException && !cancellationToken.IsCancellationRequested);
}

public class ApiResult<T> : ApiResult
{
    public T? Value { get; init; }

    public static ApiResult<T> Ok(HttpStatusCode status, T value) =>
        new() { Success = true, StatusCode = (int)status, Value = value };

    public static new ApiResult<T> Fail(HttpStatusCode status, ProblemDetailsDto? problem) =>
        new() { StatusCode = (int)status, Problem = problem };

    public static new ApiResult<T> NetworkFailure() => new() { StatusCode = 0 };
}
