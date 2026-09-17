using FitTrack.ViewModels;

namespace FitTrack.Views;

public partial class GoalDetailPage : ContentPage
{
    public GoalDetailPage(GoalDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}