using FitTrack.Api.Auth;
using FitTrack.Api.DTOs;
using FitTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace FitTrack.Api.Controllers;

/// <summary>Issues JWTs. Rate limited so a password cannot be guessed quickly.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokens;
    private readonly AuthSettings _settings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokens, IOptions<AuthSettings> settings, ILogger<AuthController> logger)
    {
        _tokens = tokens;
        _settings = settings.Value;
        _logger = logger;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult Login(LoginRequest request)
    {
        if (!_settings.Enabled)
        {
            return Problem(
                title: "Authentication is turned off",
                detail: "Set Authentication:Enabled to true to use login.",
                statusCode: StatusCodes.Status404NotFound);
        }

        var response = _tokens.Login(request.Username!, request.Password!);
        if (response is null)
        {
            // Never log the user name or password that was tried.
            _logger.LogWarning("Login failed");
            return Problem(
                title: "Invalid credentials",
                detail: "The user name or password is wrong.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        _logger.LogInformation("Login succeeded");
        return Ok(response);
    }
}
