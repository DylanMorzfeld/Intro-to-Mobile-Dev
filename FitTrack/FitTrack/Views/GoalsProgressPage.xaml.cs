using FitTrack.ViewModels;

namespace FitTrack.Views;

public partial class GoalsProgressPage : ContentPage
{
    private readonly GoalsViewModel _viewModel;

    public GoalsProgressPage(GoalsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadGoalsCommand.ExecuteAsync(null);
    }
}