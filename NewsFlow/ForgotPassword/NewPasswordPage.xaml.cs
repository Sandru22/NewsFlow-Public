using System.Net.Http.Json;
using System.Net;
using NewsFlow.Services;

namespace NewsFlow.ForgotPassword;

public partial class NewPasswordPage : ContentPage
{
    private readonly string _email;
    private readonly string _token;

    public NewPasswordPage(string email, string token)
    {
        InitializeComponent();
        _email = email;
        _token = token;
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            await DisplayAlert("Eroare", "Introdu ambele câmpuri.", "OK");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Eroare", "Parolele nu coincid.", "OK");
            return;
        }

        if (PasswordEntry.Text.Length < 6)
        {
            await DisplayAlert("Eroare", "Parola trebuie să aibă minim 6 caractere.", "OK");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync(
                $"{AppConfig.ApiBaseUrl}/auth/reset-password",
                new
                {
                    Email = _email,
                    Token = _token,
                    NewPassword = PasswordEntry.Text
                });

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Succes", "Parola a fost resetată!", "OK");
                await Navigation.PopToRootAsync();
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Eroare", error.Contains("Invalid token") ?
                    "Codul a expirat. Solicită unul nou." :
                    "Parolă invalidă.", "OK");
            }
            else
            {
                await DisplayAlert("Eroare", "A apărut o eroare necunoscută.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", $"Eroare de conexiune: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
}