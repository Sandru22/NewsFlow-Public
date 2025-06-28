using NewsFlow.News;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Text;
using NewsFlow.Services;
using NewsFlow.Models;
using System.IdentityModel.Tokens.Jwt;

namespace NewsFlow.Web;



public partial class WebViewPage : ContentPage
{
    private readonly string _url;
    private readonly NewsItem _newsItem;
    private CancellationTokenSource _ttsCts;
    public string Site { get; set; }
    public WebViewPage(NewsItem news)
    {
        _newsItem = news ?? throw new ArgumentNullException(nameof(news));
        _url = news.Url;
        InitializeComponent();
        

#if WINDOWS
        Shell.SetNavBarIsVisible(this, false);
#endif

        NewsWebView.Source = news.Url;
        Debug.WriteLine("WebViewPage: " + news.Url);

        Site = ExtractSiteFromUrl(_url); 
        Debug.WriteLine("Site" + Site);
        BindingContext = _newsItem;


    }

    private string ExtractSiteFromUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var segments = uri.AbsolutePath
                .Trim('/')
                .Split('/')
                .Where(s => !string.Equals(s, "rss", StringComparison.OrdinalIgnoreCase))
                .Where(s => !string.Equals(s, "feed", StringComparison.OrdinalIgnoreCase))
                .Where(s => !s.Contains("-"))
                .Where(s => !string.Equals(s, "stiri", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (segments.Length > 0)
                return $"{uri.Host}/{string.Join("/", segments)}";
            else
                return uri.Host;
        }

        return string.Empty;
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
            StartButton.IsVisible = !value;
        }
    }

    private void StopButton_Clicked(object sender, EventArgs e)
    {
        _ttsCts?.Cancel();
        IsTtsActive = false;
    }

    private async void OnSubscribeClicked(object sender, EventArgs e)
    {
        var token = await SecureStorage.GetAsync("auth_token");
        if (string.IsNullOrEmpty(token))
        {
            await DisplayAlert("Eroare", "Trebuie să fii autentificat pentru a te abona!", "OK");
            return;
        }

        string userId = GetUserIdFromToken(token);
        if (string.IsNullOrEmpty(userId))
        {
            await DisplayAlert("Eroare", "Nu s-a putut obține ID-ul utilizatorului!", "OK");
            return;
        }

        var newsService = new NewsApiService();
        var success = await newsService.SubscribeUnsubscribeAsyc(userId, _newsItem, token);

        if (success)
        {
            _newsItem.HasSubscribed = !_newsItem.HasSubscribed;
            await DisplayAlert("Succes", _newsItem.HasSubscribed ? "Te-ai abonat!" : "Dezabonare realizată!", "OK");
        }
        else
        {
            await DisplayAlert("Eroare", "A apărut o problemă la abonare.", "OK");
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
            if (roLocale == null)
            {
#if ANDROID
                await DisplayAlert("Limba indisponibilă", "Text-to-speech în limba română nu este disponibilă pe acest dispozitiv.\n Pentru a instala limba română accesaţi : \n - Settings > Accessibility > Text-to-speech \n sau \n - Setări > Accesibilitate > Transformare text în vorbire", "OK");
#endif
#if WINDOWS
                await DisplayAlert("Limba indisponibilă", "Text-to-speech în limba română nu este disponibilă pe acest dispozitiv.\n Pentru a instala limba română accesaţi : \n - Settings > Time & Language > Speech > Add voices ", "OK");
#endif

                return;
            }
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
