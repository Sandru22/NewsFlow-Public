using NewsFlow.Models;
using System.Net.Http.Json;

namespace NewsFlow.ForgotPassword;

public partial class ForgotPasswordPage : ContentPage
{
	public ForgotPasswordPage()
	{
		InitializeComponent();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;
        var httpClient = new HttpClient();
        var response = await httpClient.PostAsJsonAsync(
            $"{AppConfig.ApiBaseUrl}/auth/forgot-password",
            new { Email = email });

        if (response.IsSuccessStatusCode)
        {
            await Navigation.PushAsync(new EnterCodePage(email));
        }


    }

   
}