using Microsoft.Maui.Storage;
using NewsFlow.Login;
using NewsFlow.News;
using System.Threading.Tasks;

namespace NewsFlow
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoadingPage());
        }
    }

    public class LoadingPage : ContentPage
    {
        public LoadingPage()
        {
            // Optionally add a loading indicator
            Content = new ActivityIndicator
            {
                IsRunning = true,
                Color = Colors.Blue
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CheckLoginStatus();
        }

        private async Task CheckLoginStatus()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                bool rememberMe = Preferences.Get("remember_me", false);

                // Ensure navigation happens on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!string.IsNullOrEmpty(token) && rememberMe)
                    {
                        Application.Current.MainPage = new AppShell();
                    }
                    else
                    {
                        Application.Current.MainPage = new NavigationPage(new LoginPage());
                    }
                });
            }
            catch
            {
                // Fallback to Login if there's an error
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                });
            }
        }
    }
}