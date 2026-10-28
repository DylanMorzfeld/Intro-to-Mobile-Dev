using FitTrack.Services.Api;

namespace FitTrack.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Logs in and saves the token. 401 = wrong credentials, 429 = too many attempts.</summary>
    Task<ApiResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}
