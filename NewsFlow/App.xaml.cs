using Microsoft.Maui.Storage;
using NewsFlow.Login;
using NewsFlow.Models;
using NewsFlow.News;
using NewsFlow.Services;
using NewsFlow.Web;
using Plugin.Firebase.CloudMessaging;
using Plugin.FirebasePushNotification;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace NewsFlow
{
    public partial class App : Application
    {
#if ANDROID
        private string _pendingNotificationUrl = null;
        private string _pendingNotificationSource = null;
        private string _pendingNotificationnewsId = "0"; // Initialize with a default value

#endif
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoadingPage());
#if ANDROID
            CrossFirebaseCloudMessaging.Current.NotificationTapped += async (s, e) =>
            {
                Debug.WriteLine("🔔 Notification tapped");
                var url = e.Notification?.Data?["url"];
                var source = e.Notification?.Data?["source"];
                var newsId = e.Notification?.Data?["newsId"] ?? "0"; // Get newsId from notification data

                if (!string.IsNullOrEmpty(url))
                {
                    // Salvează URL-ul pentru procesare ulterioară
                    _pendingNotificationUrl = url;
                    _pendingNotificationSource = source;
                    _pendingNotificationnewsId = newsId; // Get newsId from notification data

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await HandleNotificationNavigation(url, source, Int32.Parse(newsId));
                    });
                }

            };
#endif
        }

        private async Task HandleNotificationNavigation(string url, string source, int newsId)
        {
            try
            {

                if (Current.MainPage is AppShell)
                {
                    await NavigateToWebView(url, source, newsId);
                }
                else
                {

                    Debug.WriteLine("📱 App not fully loaded, URL saved for later navigation");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Navigation error: {ex.Message}");
            }
        }

        private async Task NavigateToWebView(string url, string source, int newsId)
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var userId = GetUserIdFromToken(token); // Presupunem că ai această metodă

                var newsService = new NewsApiService();
                var news = await newsService.GetNewsByIdAsync(newsId, userId, token);

                if (news == null)
                {
                    await Current.MainPage.DisplayAlert("Eroare", "Nu s-a putut încărca știrea.", "OK");
                    return;
                }

                await Current.MainPage.Navigation.PushAsync(new WebViewPage(news));
                Debug.WriteLine("✅ Successfully navigated to WebView");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ WebView navigation failed: {ex.Message}");
                await Current.MainPage.DisplayAlert("Error", $"Navigation failed: {ex.Message}", "OK");
            }
        }

        private string GetUserIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                return jwtToken?.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;
            }
            catch
            {
                return null;
            }
        }

#if ANDROID
        public async Task ProcessPendingNotification()
        {
            if (!string.IsNullOrEmpty(_pendingNotificationUrl))
            {
                var url = _pendingNotificationUrl;
                _pendingNotificationUrl = null; // Clear pending URL

                var source = _pendingNotificationSource;
                _pendingNotificationSource = null; // Clear pending source

                var newsId = _pendingNotificationnewsId;
                _pendingNotificationnewsId = "0"; // Reset to default value

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await NavigateToWebView(url,source,Int32.Parse(newsId));
                });
            }

        }
#endif
    }


    public class LoadingPage : ContentPage
        {
            public LoadingPage()
            {
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
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (!string.IsNullOrEmpty(token) && rememberMe)
                        {
                            Application.Current.MainPage = new AppShell();

#if ANDROID
                            // Procesează notificarea pending după ce AppShell este setat
                            if (Application.Current is App app)
                            {
                                // Alternativă: folosește Device.StartTimer pentru procesare repetată
                                Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
                                {
                                    if (Application.Current.MainPage is AppShell)
                                    {
                                        MainThread.BeginInvokeOnMainThread(async () =>
                                        {
                                            await app.ProcessPendingNotification();
                                        });
                                        return false; // Stop timer
                                    }
                                    return true; // Continue checking
                                });
                            }
#endif
                        }
                        else
                        {
                            Application.Current.MainPage = new NavigationPage(new LoginPage());
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CheckLoginStatus error: {ex.Message}");

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Application.Current.MainPage = new NavigationPage(new LoginPage());
                    });
                }
            }
        
    }
}