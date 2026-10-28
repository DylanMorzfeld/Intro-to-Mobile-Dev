using System.Net.Http.Headers;
using FitTrack.Services.Interfaces;

namespace FitTrack.Services.Api;

/// <summary>
/// Adds "Authorization: Bearer (token)" to every request the workout client sends, when a token exists.
/// View models never touch the header themselves.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly ITokenStore _tokens;

    public AuthTokenHandler(ITokenStore tokens)
    {
        _tokens = tokens;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokens.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
