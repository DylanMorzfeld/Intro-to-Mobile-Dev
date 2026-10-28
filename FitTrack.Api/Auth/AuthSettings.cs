namespace FitTrack.Api.Auth;

/// <summary>
/// Bound from the "Authentication" section. Enabled lives in appsettings; the signing key
/// and the demo user's password hash are secrets and live in User Secrets (see --setup-auth).
/// </summary>
public class AuthSettings
{
    public bool Enabled { get; set; }
    public JwtSettings Jwt { get; set; } = new();
    public DemoUserSettings User { get; set; } = new();
}

public class JwtSettings
{
    public string Issuer { get; set; } = "FitTrack.Api";
    public string Audience { get; set; } = "FitTrack.App";
    public int ExpiresMinutes { get; set; } = 60;

    /// <summary>Base64 key. Secret: never committed.</summary>
    public string SigningKey { get; set; } = string.Empty;
}

public class DemoUserSettings
{
    public string Username { get; set; } = string.Empty;

    /// <summary>Format: iterations.saltBase64.hashBase64 (PBKDF2). Secret: never committed.</summary>
    public string PasswordHash { get; set; } = string.Empty;
}
