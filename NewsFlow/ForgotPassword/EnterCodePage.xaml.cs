using NewsFlow.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace NewsFlow.ForgotPassword;
public partial class EnterCodePage : ContentPage
{
    string _email;
    public EnterCodePage(string email)
    {
        _email = email;
        InitializeComponent();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync(
                $"{AppConfig.ApiBaseUrl}/auth/validate-reset-code",
                new
                {
                    Email = _email, 
                    Code = CodeEntry.Text
                });

            if (response.IsSuccessStatusCode)
            {
                // Citim conținutul JSON și extragem token-ul corect
                var jsonString = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (tokenResponse != null)
                {
                    await Navigation.PushAsync(new NewPasswordPage(_email, tokenResponse.Token));
                }
                else
                {
                    await DisplayAlert("Eroare", "Token invalid din server", "OK");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Eroare", $"Verificarea codului a eșuat: {errorContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", $"A apărut o eroare: {ex.Message}", "OK");
        }
    }

    // Clasă pentru deserializarea răspunsului JSON
    private class TokenResponse
    {
        public string Token { get; set; }
    }
}