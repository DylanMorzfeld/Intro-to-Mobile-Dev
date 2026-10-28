namespace FitTrack.Api.DTOs;

/// <summary>What the API returns after a successful login. ExpiresIn is in seconds.</summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
}
