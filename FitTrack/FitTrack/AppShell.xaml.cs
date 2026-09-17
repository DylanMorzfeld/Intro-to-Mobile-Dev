using FitTrack.Views;

namespace FitTrack;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // WorkoutDetailPage isn't a ShellContent tab - it's navigated to
        // dynamically (with a WorkoutId parameter) from WorkoutLogPage.
        // Registering it as a route makes Shell.Current.GoToAsync(...) work.
        Routing.RegisterRoute(nameof(WorkoutDetailPage), typeof(WorkoutDetailPage));
        Routing.RegisterRoute(nameof(MealDetailPage), typeof(MealDetailPage));
        Routing.RegisterRoute(nameof(GoalDetailPage), typeof(GoalDetailPage));
    }
}