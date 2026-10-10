using FitTrack.Services.Interfaces;

namespace FitTrack;

public partial class App : Application
{
	// Taking IActivityFeed makes the container create the feed at launch, so it is
	// already listening for events before any page is opened.
	public App(IActivityFeed activityFeed)
	{
		_ = activityFeed;
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
