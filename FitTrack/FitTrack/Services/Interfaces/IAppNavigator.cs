namespace FitTrack.Services.Interfaces;

/// <summary>
/// Navigation and dialogs behind an interface, so view models do not depend on
/// the static Shell.Current and can be tested with a fake.
/// </summary>
public interface IAppNavigator
{
    Task GoToAsync(string route);
    Task GoToAsync(string route, IDictionary<string, object> parameters);
    Task GoBackAsync();
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}
