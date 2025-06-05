using NewsFlow.Login;
using NewsFlow.Web;

namespace NewsFlow
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(WebViewPage), typeof(WebViewPage));
        }

        private async void MenuItem_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Ești sigur că vrei să te deconectezi?", "Da", "Nu");
            if (confirm)
            {
                SecureStorage.Remove("auth_token");
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }
    }
}
