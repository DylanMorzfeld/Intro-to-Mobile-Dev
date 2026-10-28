#if DEBUG
using System.Net.Security;

namespace FitTrack.Services.Api;

/// <summary>
/// DEBUG BUILDS ONLY. The API runs with the ASP.NET Core development certificate, which
/// the emulator does not trust. This accepts that one certificate (issuer CN=localhost)
/// and still rejects every other bad certificate. It is compiled out of Release builds.
/// </summary>
public static class DevCertificateHandler
{
    public static HttpMessageHandler Create()
    {
#if ANDROID
        var handler = new Xamarin.Android.Net.AndroidMessageHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                cert is not null && cert.Issuer.Equals("CN=localhost")
                || errors == SslPolicyErrors.None
        };
        return handler;
#else
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                errors == SslPolicyErrors.None
        };
        return handler;
#endif
    }
}
#endif
