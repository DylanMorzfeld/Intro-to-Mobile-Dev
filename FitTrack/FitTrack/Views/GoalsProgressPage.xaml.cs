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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadGoalsCommand.ExecuteAsync(null);
    }
}