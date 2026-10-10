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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Fire and forget, but through a wrapper that catches everything, so an
        // exception can never escape an async void method.
        _ = LoadSafeAsync();
    }

    private async Task LoadSafeAsync()
    {
        try
        {
            // Refreshes every time the page appears so the list stays in sync
            // after returning from adding, editing, or deleting.
            await _viewModel.LoadWorkoutsCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
