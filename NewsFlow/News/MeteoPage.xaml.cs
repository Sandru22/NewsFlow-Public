using NewsFlow.ViewModels;

namespace NewsFlow.News;

public partial class MeteoPage : ContentPage
{
	public MeteoPage()
	{
		InitializeComponent();
#if WINDOWS
        Shell.SetNavBarIsVisible(this, false);
#endif
        BindingContext = new NewsViewModel("Meteo");
    }


    private async void OnCollectionViewRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        var viewModel = (NewsViewModel)BindingContext;
        if (viewModel.LoadMoreNewsCommand.CanExecute(null))
        {
            viewModel.LoadMoreNewsCommand.Execute(null);
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var viewModel = (NewsViewModel)BindingContext;
        if (viewModel.RefreshCommand.CanExecute(null))
        {
            viewModel.RefreshCommand.Execute(null);
        }
    }
}