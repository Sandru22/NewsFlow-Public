using NewsFlow.News;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Text;
using NewsFlow.Services;
namespace NewsFlow.Web;



public partial class WebViewPage : ContentPage
{
    private readonly string _url;
    private CancellationTokenSource _ttsCts;
    public WebViewPage(string url)
    {
        _url = url;
        InitializeComponent();
        NewsWebView.Source = url;
        Debug.WriteLine("WebViewPage: " + url);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _ttsCts?.Cancel();
    }

    private bool _isTtsActive = false;

    public bool IsTtsActive
    {
        get => _isTtsActive;
        set
        {
            _isTtsActive = value;
            StopButton.IsVisible = value;
        }
    }

    private void StopButton_Clicked(object sender, EventArgs e)
    {
        _ttsCts?.Cancel();
        IsTtsActive = false;
    }
    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
           
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(_url);

           string plainText = System.Net.WebUtility.HtmlDecode(html);


            var doc = new HtmlDocument();
            doc.LoadHtml(plainText);

            var paragraphs = doc.DocumentNode
                .SelectNodes("//p")
                ?.Select(p => p.InnerText.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p));

            var fullText = string.Join("\n\n", paragraphs ?? Enumerable.Empty<string>());



            Debug.WriteLine(fullText);

            if (string.IsNullOrWhiteSpace(fullText))
            {
                await DisplayAlert("Eroare", "Nu s-a putut extrage conținutul.", "OK");
                return;
            }

            _ttsCts?.Cancel();
            _ttsCts = new CancellationTokenSource();
            IsTtsActive = true;
            var locales = await TextToSpeech.GetLocalesAsync();
            var roLocale = locales?.FirstOrDefault(l => l.Language.StartsWith("ro"));

            await TextToSpeechService.SpeakAsync(fullText, _ttsCts.Token);

            IsTtsActive = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Eroare la citirea articolului: {ex.Message}");
            await DisplayAlert("Eroare", "A apărut o problemă la citirea articolului.", "OK");
        }
    }

}
