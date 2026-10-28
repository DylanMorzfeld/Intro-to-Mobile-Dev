using System.ComponentModel.DataAnnotations;

namespace FitTrack.Api.DTOs;

/// <summary>What a client sends to log in.</summary>
public class LoginRequest
{
    [Required(ErrorMessage = "A user name is required.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "A password is required.")]
    public string? Password { get; set; }
}
