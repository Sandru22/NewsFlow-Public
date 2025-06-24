using NewsFlow.Models;
using NewsFlow.ViewModels;

namespace NewsFlow.News;

public partial class SubscriptionsPage : ContentPage
{
	public SubscriptionsPage()
	{
		InitializeComponent();
#if WINDOWS
        Shell.SetNavBarIsVisible(this, false);
#endif
        var viewModel = new SubscribedNewsViewModel();
        viewModel.ScrollToItemCallback = ScrollToItem;
        BindingContext = viewModel;
    }

    private void OnCollectionViewRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        var vm = (SubscribedNewsViewModel)BindingContext;
        if (vm.LoadMoreNewsCommand.CanExecute(null))
            vm.LoadMoreNewsCommand.Execute(null);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        var vm = (SubscribedNewsViewModel)BindingContext;
        if (vm.RefreshCommand.CanExecute(null))
            vm.RefreshCommand.Execute(null);
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