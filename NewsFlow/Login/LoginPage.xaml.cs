using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using NewsFlow.ForgotPassword;
using NewsFlow.Models;
using NewsFlow.News;
using NewsFlow.Register;
using NewsFlow.Services;
using Plugin.Firebase.CloudMessaging;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace NewsFlow.Login;

public partial class LoginPage : ContentPage
{
    private readonly AuthApiService _apiService = new();

    public LoginPage()
    {
        InitializeComponent();
        CheckLoginStatus();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var token = await _apiService.Login(EmailEntry.Text, PasswordEntry.Text);
            if (!string.IsNullOrEmpty(token))
            {
                await SecureStorage.SetAsync("auth_token", token);
                Preferences.Set("remember_me", RememberMeCheckBox.IsChecked);
#if ANDROID
                var status = await CheckAndRequestNotificationPermissionAsync();

                if (status == PermissionStatus.Granted)
                {



                    string userId = ExtractUserIdFromToken(token);


                    var deviceToken = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

                    var client = new HttpClient();
                    await client.PostAsJsonAsync($"{AppConfig.ApiBaseUrl}/News/register-device", new
                    {
                        userId = userId,
                        deviceToken = deviceToken
                    });

            }
#endif
                
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Eroare", "Autentificare eșuată. Verifică emailul și parola.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", $"A apărut o eroare:\n{ex.Message}", "OK");

        }
    }

    private async void CheckLoginStatus()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        bool rememberMe = Preferences.Get("remember_me", false);

        // Only navigate to NewsPage if the token is valid AND "Remember Me" is enabled
        if (!string.IsNullOrEmpty(token) && rememberMe && IsTokenValid(token))
        {

            Application.Current.MainPage = new NavigationPage(new NewsPage());
        }
    }

    private string ExtractUserIdFromToken(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(jwt) as JwtSecurityToken;
        return token?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
    }

    private bool IsTokenValid(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null) return false;

            return jwtToken.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }


    }

    public async Task<PermissionStatus> CheckAndRequestNotificationPermissionAsync()
    {
        await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        }
        return status;
    }


    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ForgotPasswordPage());
    }

    private async void TapGestureRecognizer_Register(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}