using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace NewsFlow.Register;

public partial class RegisterPage : ContentPage
{
    private readonly AuthApiService _apiService = new();

    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {

        if (FullNameEntry.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1)
        {
            await DisplayAlert("Eroare", "Te rugăm să introduci un singur nume (fără spații).", "OK");
            return;
        }
        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Eroare", "Parolele nu se potrivesc!", "OK");
            return;
        }

        if (!IsEmailValid(EmailEntry.Text))
        {
            await DisplayAlert("Eroare", "Adresa de e-mail nu este validă", "OK");
            return;
        }
        else if (!IsPassWordValid(PasswordEntry.Text))
        {
            await DisplayAlert("Eroare", "Parola trebuie să conțină:\n- cel puțin 8 caractere\n- o literă mare\n- o literă mică\n- o cifră\n- un caracter special (!@#$%^&*)", "OK");
            return;
        }

        var success = await _apiService.Register(FullNameEntry.Text, EmailEntry.Text, PasswordEntry.Text);
        if (success)
        {
            await DisplayAlert("Succes", "Utilizator creat!", "OK");
            await Navigation.PopAsync();
            
        }
            else
            {
                await DisplayAlert("Eroare", "Înregistrare eşuată!", "OK");
            }
    }
    

    private bool IsEmailValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    private bool IsPassWordValid(string Password)
    {
        if (string.IsNullOrEmpty(Password) || Password.Length < 8)
        {
            return false;
        }
        var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,}$");
        return regex.IsMatch(Password);
    } 
}