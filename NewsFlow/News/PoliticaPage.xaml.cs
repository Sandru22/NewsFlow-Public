using NewsFlow.Login;
using NewsFlow.Models;
using NewsFlow.ViewModels;

namespace NewsFlow.News;

public partial class PoliticaPage : ContentPage
{
	public PoliticaPage()
	{
		InitializeComponent();
#if WINDOWS
        Shell.SetNavBarIsVisible(this, false);
#endif
        var viewModel = new NewsViewModel("politica");
        viewModel.ScrollToItemCallback = ScrollToItem;
        BindingContext = viewModel;
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

    public void ScrollToItem(NewsItem item)
    {
        if (item == null) return;

#if ANDROID || IOS
    NewsListView_Android.ScrollTo(item, position: ScrollToPosition.Start, animate: true);
#elif WINDOWS
        NewsListView_Windows.ScrollTo(item, position: ScrollToPosition.Start, animate: true);
#endif
    }
}