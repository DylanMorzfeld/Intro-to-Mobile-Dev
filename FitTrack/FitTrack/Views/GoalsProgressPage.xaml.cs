using FitTrack.ViewModels;

namespace FitTrack.Views;

public partial class GoalsProgressPage : ContentPage
{
    private readonly GoalsViewModel _viewModel;

    public GoalsProgressPage(GoalsViewModel viewModel)
    {
        // Set the BindingContext before InitializeComponent so compiled/typed XAML
        // bindings that use AncestorType or the page's ViewModel won't be invoked
        // with a null source during component initialization.
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
            await _viewModel.LoadGoalsCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
