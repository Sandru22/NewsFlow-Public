using NewsFlow.Login;
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
        BindingContext = new NewsViewModel();
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

