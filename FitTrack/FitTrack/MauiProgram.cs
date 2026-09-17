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

        // --- Services ---------------------------------------------------
        // Registered as a Singleton because SQLiteAsyncConnection is safe
        // to share across the whole app, and we want exactly one shared
        // database connection rather than opening a new one per page.
        //
        // This is the ONE line that changes when a real backend API is
        // added later: swap LocalWorkoutRepository for something like
        // ApiWorkoutRepository, and every ViewModel that depends on
        // IWorkoutRepository keeps working without any changes.
        builder.Services.AddSingleton<IWorkoutRepository, LocalWorkoutRepository>();
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


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}