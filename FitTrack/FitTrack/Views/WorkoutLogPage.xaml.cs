using FitTrack.ViewModels;

namespace FitTrack.Views;

public partial class WorkoutLogPage : ContentPage
{
    private readonly WorkoutLogViewModel _viewModel;

    // The ViewModel is injected via the DI container (registered in
    // MauiProgram.cs), keeping this code-behind free of manual object
    // construction or business logic - its only job is wiring the View
    // to its ViewModel.
    public WorkoutLogPage(WorkoutLogViewModel viewModel)
    {
        // Ensure the BindingContext is set before InitializeComponent runs so
        // compiled/typed bindings (generated from XAML) receive a non-null
        // source when evaluated during component initialization.
        _viewModel = viewModel;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Refresh the list every time the page appears, so it stays in
        // sync after returning from adding/editing/deleting a workout.
        await _viewModel.LoadWorkoutsCommand.ExecuteAsync(null);
    }
}