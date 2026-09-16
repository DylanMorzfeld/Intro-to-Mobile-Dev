using FitTrack.ViewModels;

namespace FitTrack.Views;

public partial class WorkoutDetailPage : ContentPage
{
    public WorkoutDetailPage(WorkoutDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}