using System.Net;
using System.Net.Http.Json;
using FitTrack.Services.Interfaces;

namespace FitTrack.Services.Api;

/// <summary>
/// Talks to POST api/auth/login. Uses its own HttpClient WITHOUT the bearer handler:
/// logging in is how you get a token, so there is none to send yet.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokens;

    public AuthService(HttpClient http, ITokenStore tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    public async Task<ApiResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(
                "api/auth/login", new LoginRequestDto(username, password), ApiJson.Options, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return ApiResult.Fail(response.StatusCode, await ApiJson.ReadProblemAsync(response, cancellationToken));

            var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>(ApiJson.Options, cancellationToken);
            if (login is null || string.IsNullOrEmpty(login.AccessToken))
                return ApiResult.Fail(HttpStatusCode.InternalServerError, null);

            // Expire the saved token 30 seconds early so it is never sent right as it runs out.
            await _tokens.SaveAsync(login.AccessToken, DateTime.UtcNow.AddSeconds(Math.Max(0, login.ExpiresIn - 30)));
            return ApiResult.Ok(response.StatusCode);
        }
        catch (Exception ex) when (ApiResult.IsNetworkFailure(ex, cancellationToken))
        {
            return ApiResult.NetworkFailure();
        }
    }
}
