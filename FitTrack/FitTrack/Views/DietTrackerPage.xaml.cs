using FitTrack.ViewModels;

namespace FitTrack.Views;

public partial class DietTrackerPage : ContentPage
{
    private readonly DietViewModel _viewModel;

    public DietTrackerPage(DietViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadMealsCommand.ExecuteAsync(null);
    }
}