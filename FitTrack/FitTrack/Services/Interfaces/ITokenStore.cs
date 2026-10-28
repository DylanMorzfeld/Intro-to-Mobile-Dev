namespace FitTrack.Services.Interfaces;

/// <summary>Where the login token is kept. The token is never logged and never shown on screen.</summary>
public interface ITokenStore
{
    /// <summary>The saved token, or null if there is none or it has expired.</summary>
    Task<string?> GetTokenAsync();

    Task SaveAsync(string token, DateTime expiresUtc);

    Task ClearAsync();
}
