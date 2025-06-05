using Microsoft.Maui.Controls;
using System.ComponentModel;

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
        if(PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Eroare", "Parolele nu se potrivesc!", "OK");
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
            if (!IsPassWordValid(PasswordEntry.Text))
            {
                await DisplayAlert("Eroare", "Parola trebuie să conțină:\n- cel puțin 8 caractere\n- o literă mare\n- o literă mică\n- o cifră\n- un caracter special (!@#$%^&*)", "OK");
            }
            else
            {
                await DisplayAlert("Eroare", "Înregistrare eşuată!", "OK");
            }
        }
    }

    private bool IsPassWordValid(string Password)
    {
        if (string.IsNullOrEmpty(Password) || Password.Length < 8)
        {
            return false;
        }
        bool HaseUpper = Password.Any(char.IsUpper);
        bool HaseLower = Password.Any(char.IsLower);
        bool HaseNumber = Password.Any(char.IsNumber);
        bool HaseSpecial = Password.Any(ch => "!@#$%^&*()-_=+[]{}|;:'\",.<>?/\\`~".Contains(ch));


        return HaseUpper && HaseLower && HaseNumber && HaseSpecial;
    } 
}