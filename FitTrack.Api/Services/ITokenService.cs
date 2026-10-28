using FitTrack.Api.DTOs;

namespace FitTrack.Api.Services;

public interface ITokenService
{
    /// <summary>Returns a token when the credentials are right, otherwise null.</summary>
    LoginResponse? Login(string username, string password);
}
