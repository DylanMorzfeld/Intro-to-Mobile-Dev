using System.Globalization;
using FitTrack.Services.Interfaces;

namespace FitTrack.Services.Api;

/// <summary>
/// Keeps the token in SecureStorage (Android Keystore, iOS Keychain, Windows data protection),
/// never in Preferences or a plain file. If SecureStorage is unavailable on a platform, an
/// in-memory copy keeps the app working for the current run.
/// </summary>
public class SecureTokenStore : ITokenStore
{
    private const string TokenKey = "fittrack.access_token";
    private const string ExpiryKey = "fittrack.access_token_expires";

    private string? _memoryToken;
    private DateTime _memoryExpiresUtc;

    public async Task SaveAsync(string token, DateTime expiresUtc)
    {
        _memoryToken = token;
        _memoryExpiresUtc = expiresUtc;

        try
        {
            await SecureStorage.Default.SetAsync(TokenKey, token);
            await SecureStorage.Default.SetAsync(ExpiryKey, expiresUtc.ToString("O", CultureInfo.InvariantCulture));
        }
        catch (Exception ex)
        {
            // Do not print the token. The exception type is enough to diagnose.
            System.Diagnostics.Debug.WriteLine($"SecureStorage write failed: {ex.GetType().Name}");
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        var token = _memoryToken;
        var expiresUtc = _memoryExpiresUtc;

        if (token is null)
        {
            try
            {
                token = await SecureStorage.Default.GetAsync(TokenKey);
                var rawExpiry = await SecureStorage.Default.GetAsync(ExpiryKey);

                if (token is not null &&
                    DateTime.TryParse(rawExpiry, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
                {
                    expiresUtc = parsed;
                    _memoryToken = token;
                    _memoryExpiresUtc = parsed;
                }
                else
                {
                    token = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SecureStorage read failed: {ex.GetType().Name}");
                token = null;
            }
        }

        if (token is null || expiresUtc <= DateTime.UtcNow)
        {
            await ClearAsync();
            return null;
        }

        return token;
    }

    public Task ClearAsync()
    {
        _memoryToken = null;
        _memoryExpiresUtc = default;

        try
        {
            SecureStorage.Default.Remove(TokenKey);
            SecureStorage.Default.Remove(ExpiryKey);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SecureStorage clear failed: {ex.GetType().Name}");
        }

        return Task.CompletedTask;
    }
}
