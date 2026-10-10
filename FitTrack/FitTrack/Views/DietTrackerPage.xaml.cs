using FitTrack.ViewModels;

namespace FitTrack.Views;

public partial class DietTrackerPage : ContentPage
{
    private readonly DietViewModel _viewModel;

    public DietTrackerPage(DietViewModel viewModel)
    {
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
            await _viewModel.LoadMealsCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
