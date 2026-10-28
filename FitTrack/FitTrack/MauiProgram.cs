using CommunityToolkit.Mvvm.Messaging;
using FitTrack.Services;
using FitTrack.Services.Api;
using FitTrack.Services.Interfaces;
using FitTrack.Services.Local;
using FitTrack.ViewModels;
using FitTrack.Views;
using Microsoft.Extensions.Logging;

namespace FitTrack;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- API client (workouts) -------------------------------------
        // Workouts now live behind the API. The app never opens a SQL connection;
        // the API is the only thing that talks to SQL Server.
        //
        // The token is kept in SecureStorage. AuthTokenHandler adds it as a bearer header to
        // every workout request. The login client has NO token handler: there is no token yet.
        builder.Services.AddSingleton<ITokenStore, SecureTokenStore>();
        builder.Services.AddTransient<AuthTokenHandler>();

        var workoutClient = builder.Services
            .AddHttpClient<IWorkoutApiClient, WorkoutApiClient>(ConfigureClient)
            .AddHttpMessageHandler<AuthTokenHandler>();

        var authClient = builder.Services
            .AddHttpClient<IAuthService, AuthService>(ConfigureClient);

#if DEBUG
        // Debug builds only: trust the ASP.NET Core development certificate (see DevCertificateHandler).
        workoutClient.ConfigurePrimaryHttpMessageHandler(() => DevCertificateHandler.Create());
        authClient.ConfigurePrimaryHttpMessageHandler(() => DevCertificateHandler.Create());
#endif

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        // Transient: a new instance is created each time the page is navigated to,
        // which is the right lifetime for page-scoped state like this.
        builder.Services.AddTransient<WorkoutLogViewModel>();
        builder.Services.AddTransient<WorkoutLogPage>();
        builder.Services.AddTransient<WorkoutDetailViewModel>();
        builder.Services.AddTransient<WorkoutDetailPage>();
        builder.Services.AddSingleton<IDietRepository, LocalDietRepository>();
        builder.Services.AddTransient<DietViewModel>();
        builder.Services.AddTransient<MealDetailViewModel>();
        builder.Services.AddTransient<DietTrackerPage>();
        builder.Services.AddTransient<MealDetailPage>();
        builder.Services.AddSingleton<IGoalRepository, LocalGoalRepository>();
        builder.Services.AddTransient<GoalsViewModel>();
        builder.Services.AddTransient<GoalDetailViewModel>();
        builder.Services.AddTransient<GoalsProgressPage>();
        builder.Services.AddTransient<GoalDetailPage>();
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        builder.Services.AddSingleton<IActivityFeed, ActivityFeedService>();
        builder.Services.AddSingleton<IAppNavigator, ShellAppNavigator>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void ConfigureClient(HttpClient client)
    {
        client.BaseAddress = new Uri(ApiSettings.BaseAddress);
        client.Timeout = TimeSpan.FromSeconds(15);
    }
}