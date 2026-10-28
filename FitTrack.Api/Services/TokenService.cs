using System.Security.Cryptography;
using FitTrack.Api.Auth;
using FitTrack.Api.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FitTrack.Api.Services;

/// <summary>Checks the demo user's credentials and issues a signed JWT.</summary>
public class TokenService : ITokenService
{
    private readonly AuthSettings _settings;

    public TokenService(IOptions<AuthSettings> settings)
    {
        _settings = settings.Value;
    }

    public LoginResponse? Login(string username, string password)
    {
        // Always hash, even for a wrong user name, so the time taken does not reveal which part was wrong.
        var passwordMatches = PasswordMatches(password, _settings.User.PasswordHash);
        var userMatches = string.Equals(username, _settings.User.Username, StringComparison.OrdinalIgnoreCase);

        if (!passwordMatches || !userMatches)
            return null;

        var key = new SymmetricSecurityKey(Convert.FromBase64String(_settings.Jwt.SigningKey));
        var lifetime = TimeSpan.FromMinutes(_settings.Jwt.ExpiresMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Jwt.Issuer,
            Audience = _settings.Jwt.Audience,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = _settings.User.Username,
                ["name"] = _settings.User.Username
            },
            Expires = DateTime.UtcNow.Add(lifetime),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new LoginResponse
        {
            AccessToken = new JsonWebTokenHandler().CreateToken(descriptor),
            TokenType = "Bearer",
            ExpiresIn = (int)lifetime.TotalSeconds
        };
    }

    private static bool PasswordMatches(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
