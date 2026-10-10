using FitTrack.Services.Interfaces;

namespace FitTrack.Services;

/// <summary>
/// The only class in the app that talks to Shell.Current for navigation and dialogs.
/// </summary>
public class ShellAppNavigator : IAppNavigator
{
    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);

    public Task GoToAsync(string route, IDictionary<string, object> parameters)
        => Shell.Current.GoToAsync(route, parameters);

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
        => Shell.Current.DisplayAlert(title, message, accept, cancel);
}
