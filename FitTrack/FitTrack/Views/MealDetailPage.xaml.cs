using FitTrack.ViewModels;

namespace FitTrack.Views;

public partial class MealDetailPage : ContentPage
{
    public MealDetailPage(MealDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}