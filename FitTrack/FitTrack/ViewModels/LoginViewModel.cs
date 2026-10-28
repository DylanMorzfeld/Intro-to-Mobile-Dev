using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitTrack.Services.Interfaces;

namespace FitTrack.ViewModels;

/// <summary>
/// Backs LoginPage. The list page sends the user here when they try to change data without a
/// valid token. After a successful login the token is in SecureStorage and the page closes.
/// </summary>
[QueryProperty(nameof(Message), "Message")]
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IAppNavigator _navigator;

    /// <summary>Why the user was sent here, e.g. "Please log in to make changes."</summary>
    [ObservableProperty]
    private string? message;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isBusy;

    public LoginViewModel(IAuthService auth, IAppNavigator navigator)
    {
        _auth = auth;
        _navigator = navigator;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Enter a user name and a password.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var result = await _auth.LoginAsync(Username.Trim(), Password);

            if (result.Success)
            {
                // Do not keep the password in memory (or on screen) longer than needed.
                Password = string.Empty;
                await _navigator.GoBackAsync();
                return;
            }

            ErrorMessage = result.StatusCode switch
            {
                401 => "Wrong user name or password.",
                404 => "Login is turned off on the API (authentication is disabled).",
                429 => "Too many attempts. Wait a minute and try again.",
                _ => result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            // Never log the user name or password.
            System.Diagnostics.Debug.WriteLine($"Login failed unexpectedly: {ex.GetType().Name}");
            ErrorMessage = "Something went wrong. Try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        Password = string.Empty;
        await _navigator.GoBackAsync();
    }
}
