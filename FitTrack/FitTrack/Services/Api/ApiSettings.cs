namespace FitTrack.Services.Api;

/// <summary>
/// Where the API lives. The Android emulator cannot reach the PC as "localhost" (that
/// means the emulator itself); 10.0.2.2 is the emulator's alias for the PC.
/// The port matches the https profile in FitTrack.Api/Properties/launchSettings.json.
/// The trailing slash matters: relative URLs like "api/workouts" are appended to it.
/// </summary>
public static class ApiSettings
{
    private const int Port = 7059;

    public static string BaseAddress => DeviceInfo.Platform == DevicePlatform.Android
        ? $"https://10.0.2.2:{Port}/"
        : $"https://localhost:{Port}/";
}
