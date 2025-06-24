using NewsFlow.Login;
using NewsFlow.Services;
using NewsFlow.Web;
using Plugin.Firebase.CloudMessaging;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

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

#if ANDROID
                var token = await SecureStorage.GetAsync("auth_token");
                string userId = ExtractUserIdFromToken(token);


                var deviceToken = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

                var client = new HttpClient();
                await client.PostAsJsonAsync($"{AppConfig.ApiBaseUrl}/News/unregister-device", new
                {
                    userId = userId,
                    deviceToken = deviceToken
                });
#endif
                SecureStorage.Remove("auth_token");
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        private string ExtractUserIdFromToken(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(jwt) as JwtSecurityToken;
            return token?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        }
    }
}
