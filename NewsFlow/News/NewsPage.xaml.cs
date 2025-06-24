using NewsFlow.Login;
using NewsFlow.Models;
using NewsFlow.ViewModels;
using System.Windows.Input;

namespace NewsFlow.News;

public partial class NewsPage : ContentPage
{

    public NewsPage()
    {
        InitializeComponent();
#if WINDOWS
        Shell.SetNavBarIsVisible(this, false);
#endif
        var viewModel = new NewsViewModel();
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

    private async void OnSearchPressed(object sender, EventArgs e)
    {
        var query = SearchBar.Text;
        var viewModel = (NewsViewModel)BindingContext;
        await viewModel.SearchNewsAsync(query);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        // Dacă utilizatorul șterge tot, reîncarcă știrile normale
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            var viewModel = (NewsViewModel)BindingContext;
            _ = viewModel.ExecuteRefresh();
        }
    }
}

