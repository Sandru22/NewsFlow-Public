using NewsFlow.Models;
using NewsFlow.Web;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using NewsFlow.Services;

using NewsFlow.News;
using NewsFlow.ViewModels;
using Microsoft.Maui.ApplicationModel.DataTransfer;


namespace NewsFlow.ViewModels;

public class NewsViewModel : BindableObject
{
    private readonly HttpClient _httpClient = new();
    public ObservableCollection<NewsItem> News { get; set; } = new();
    public ICommand LikeNewsCommand { get; private set; }
    public ICommand OpenNewsCommand { get; private set; }
    public ICommand ShareNewsCommand { get; private set; }
    public ICommand ReadNewsCommand { get; private set; }
    public ICommand LoadMoreNewsCommand { get; private set; }
    public ICommand RefreshCommand { get; private set; }
    public ICommand SubscribeCommand { get; private set; }

    private readonly NewsApiService _newsService = new();

    public Action<NewsItem>? ScrollToItemCallback { get; set; }
    private bool _isTtsControlVisible;
    public bool IsTtsControlVisible
    {
        get => _isTtsControlVisible;
        set
        {
            _isTtsControlVisible = value;
            OnPropertyChanged();
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            ((Command)LoadMoreNewsCommand).ChangeCanExecute();
        }
    }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }


    private async Task RefreshNews()
    {
        try
        {
            _currentPage = 1;
            HasMoreItems = true;
            News.Clear();
            _newsListForTTS.Clear();
            await LoadMoreNews();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Eroare la refresh: {ex.Message}");
            if (!await HasInternetConnectionAsync())
            {
                await Application.Current.MainPage.DisplayAlert("Fără internet", "Nu există conexiune la internet.", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Eroare", "A apărut o problemă la reîmprospătarea știrilor", "OK");
            }
        }
    }

    public async Task ExecuteRefresh()
    {
        if (IsLoading)
        {
            IsRefreshing = false;
            return;
        }

        if (!await HasInternetConnectionAsync())
        {
            IsRefreshing = false;
            await Application.Current.MainPage.DisplayAlert("Fără internet", "Nu putem reîmprospăta știrile. Verifică conexiunea la rețea.", "OK");
            return;
        }

        try
        {
            IsRefreshing = true;
            await RefreshNews();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Refresh error: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private bool _hasMoreItems = true;
    public bool HasMoreItems
    {
        get => _hasMoreItems;
        set
        {
            _hasMoreItems = value;
            OnPropertyChanged(nameof(HasMoreItems)); 
            ((Command)LoadMoreNewsCommand).ChangeCanExecute();
        }
    }

    private int _currentPage = 1;
    private int _currentNewsIndex = 0;
    private CancellationTokenSource _ttsCancellationTokenSource;
    private List<NewsItem> _newsListForTTS = new();

    public string Category { get; set; } = "recommended";



    public NewsViewModel(string category = "recommended")
    {
        Category = category;

        InitCommands();

        Debug.WriteLine($"NewsViewModel initialized with category: {Category}");

        _ = InitialLoad();
    }


    private bool _isReadingNews = false;
    private void InitCommands()
    {
        OpenNewsCommand = new Command<NewsItem>(async (NewsItem) => await OpenBrowser(NewsItem));
        LikeNewsCommand = new Command<int>(async (id) => await LikeNews(id));
        ShareNewsCommand = new Command<NewsItem>(async (NewsItem) => await ShareNews(NewsItem));
        LoadMoreNewsCommand = new Command(async () => await LoadMoreNews(), () => !IsLoading && HasMoreItems);
        RefreshCommand = new Command(async () => await ExecuteRefresh());
        SubscribeCommand = new Command<int>(async (id) => await SubscribeToSource(id));
        ReadNewsCommand = new Command<ObservableCollection<NewsItem>>(async newsList =>
        {

            if (_isReadingNews) return; 

            _isReadingNews = true;
            
            _newsListForTTS = newsList.ToList();
            _currentNewsIndex = 0;
            IsTtsControlVisible = true;
            await ReadNewsLoopAsync(); 
        });
    }
    private async Task<bool> HasInternetConnectionAsync()
    {
        try
        {
            var current = Connectivity.NetworkAccess;
            return current == NetworkAccess.Internet;
        }
        catch
        {
            return false;
        }
    }


    private async Task InitialLoad()
    {
        _currentPage = 1;
        HasMoreItems = true;
        News.Clear();
        await LoadMoreNews();
    }



    public ICommand StopTtsCommand => new Command(() =>
    {
        _ttsCancellationTokenSource?.Cancel();
        IsTtsControlVisible = false;
        _isReadingNews = false;
        if (_newsListForTTS != null && _newsListForTTS.Count > 0 && _currentNewsIndex >= 0 && _currentNewsIndex < _newsListForTTS.Count)
        {
            var current = _newsListForTTS[_currentNewsIndex];
            current.IsHighlighted = false;
        }
    });

    public ICommand NextNewsTtsCommand => new Command(async () =>
    {
        if (_currentNewsIndex < _newsListForTTS.Count - 1)
        {
            _ttsCancellationTokenSource?.Cancel();
            _ttsCancellationTokenSource = new CancellationTokenSource();
            _currentNewsIndex++;
            await ReadNewsLoopAsync();
        }
    });

    public ICommand PreviousNewsTtsCommand => new Command(async () =>
    {
        if (_currentNewsIndex > 0)
        {
            _ttsCancellationTokenSource?.Cancel();
            _ttsCancellationTokenSource = new CancellationTokenSource();
            _currentNewsIndex--;
            await ReadNewsLoopAsync();
        }
    });


    private async Task ReadCurrentNewsAsync()
    {

        var locales = await TextToSpeech.GetLocalesAsync();
        var roLocale = locales?.FirstOrDefault(l => l.Language.StartsWith("ro"));
        if (roLocale == null)
        {

#if ANDROID
                bool goToSettings = await Application.Current.MainPage.DisplayAlert( "Limba indisponibilă", "Text-to-speech în limba română nu este disponibilă pe acest dispozitiv.\nDoriți să deschideți setările Text-to-Speech?","Da", "Nu");

        if (goToSettings)
        {
             MainActivity.OpenTTSSettings();
        }
#endif

#if WINDOWS
            await Application.Current.MainPage.DisplayAlert("Limba indisponibilă", "Text-to-speech în limba română nu este disponibilă pe acest dispozitiv.\n Pentru a instala limba română accesaţi : \n - Settings > Time & Language > Speech > Add voices ", "OK");
#endif

            return;
        }

        try
        {
            if (_currentNewsIndex < _newsListForTTS.Count)
            {
                
                foreach (var item in News)
                    item.IsHighlighted = false;

                var current = _newsListForTTS[_currentNewsIndex];
                current.IsHighlighted = true;


                ScrollToItemCallback?.Invoke(current);
                var text = $"{current.Title}. {current.Content}";
                await TextToSpeechService.SpeakAsync(text, _ttsCancellationTokenSource.Token);
            }


        }
        catch (OperationCanceledException)
        {
            
        }
    }

    private async Task ReadNewsLoopAsync()
    {
        _ttsCancellationTokenSource = new CancellationTokenSource();

        try
        {
            while (_currentNewsIndex < _newsListForTTS.Count)
            {
                await ReadCurrentNewsAsync();

                if (!_ttsCancellationTokenSource.Token.IsCancellationRequested)
                {
                    _currentNewsIndex++;
                }
                else
                {
                    break;
                }
            }

            if (_currentNewsIndex >= _newsListForTTS.Count)
            {
                IsTtsControlVisible = false;
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    public async Task SearchNewsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return;

        try
        {
            IsLoading = true;

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            var userId = GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId)) return;

            var encoded = Uri.EscapeDataString(query);
            var url = $"{AppConfig.ApiBaseUrl}/news/search/{encoded}?userId={userId}&page=1&pageSize=20";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<NewsItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                News.Clear();
                foreach (var item in results)
                {
                    var enriched = await _newsService.PopulateNewsLikesAsync(item, userId);
                    News.Add(enriched);
                }

                HasMoreItems = false; 
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Eroare", "Căutarea a eșuat.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Eroare la căutare: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    private async Task LoadMoreNews()
    {
        if (IsLoading || !HasMoreItems)
            return;

        try
        {
            if (!await HasInternetConnectionAsync())
            {
                Debug.WriteLine("❌ Nu există conexiune la internet - anulăm încărcarea.");
                await Application.Current.MainPage.DisplayAlert("Fără internet", "Nu putem încărca știrile. Verifică conexiunea la rețea.", "OK");
                return;
            }

            IsLoading = true;

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("Tokenul nu este disponibil");
                return;
            }

            string userId = GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("Nu s-a putut obține userId din token");
                return;
            }

            var newsList = await FetchNews(userId, _currentPage);

            if (newsList == null)
            {
                Debug.WriteLine("Lista de știri este null");
                return;
            }

            Debug.WriteLine($"Loaded {newsList.Count} news items for category {Category}");

            if (newsList.Count == 0)
            {
                
                HasMoreItems = false;
                return;
            }

            foreach (var news in newsList)
            {
                
                if (News.Any(n => n.Url == news.Url || n.Title == news.Title || n.Content == news.Content))
                {
                    continue; 
                }
                var enrichedNews = await _newsService.PopulateNewsLikesAsync(news, userId);
                News.Add(enrichedNews);
            }

            _currentPage++;
            _newsListForTTS = News.ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Eroare la încărcarea știrilor: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await Application.Current.MainPage.DisplayAlert("Eroare", "A apărut o problemă la încărcarea știrilor", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }


    private async Task ShareNews(NewsItem news)
    {
        try
        {


            if (news == null) return;

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                await Application.Current.MainPage.DisplayAlert("Eroare", "Trebuie să fii autentificat!", "OK");
                return;
            }

            string userId = GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
            {
                await Application.Current.MainPage.DisplayAlert("Eroare", "Nu s-a putut obține ID-ul utilizatorului!", "OK");
                return;
            }


            var isShared = await _newsService.ShareNewsAsync(userId, news, token);

#if WINDOWS
            var shareText = $"*{news.Title}*\n{news.Content}\n\n{news.Url}";
            await Clipboard.Default.SetTextAsync(shareText);
            await Application.Current.MainPage.DisplayAlert("Distribuire", "Conințut copiat în clipboard!", "OK");
#endif

            await Share.RequestAsync(new ShareTextRequest
            {
                Text = $"{news.Title}\n\n{news.Content}\n\n{news.Url}",
                Title = "Distribuie această știre"
            });

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Eroare la share: {ex.Message}");
        }
    }

    private async Task LikeNews(int newsId)
    {

        if (!await HasInternetConnectionAsync())
        {
            
            await Application.Current.MainPage.DisplayAlert("Fără internet", "Verifică conexiunea la rețea.", "OK");
            return;
        }


        var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                await Application.Current.MainPage.DisplayAlert("Eroare", "Trebuie să fii autentificat pentru a da like!", "OK");
                return;
            }

            string userId = GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
            {
                await Application.Current.MainPage.DisplayAlert("Eroare", "Nu s-a putut obține ID-ul utilizatorului!", "OK");
                return;
            }

            var newsItem = News.FirstOrDefault(n => n.NewsId == newsId);
            if (newsItem == null) return;

            await _newsService.LikeUnLikeAsync(userId, newsItem, newsId, token);

    }


    private async Task SubscribeToSource(int newsId)
    {

        if (!await HasInternetConnectionAsync())
        {
            await Application.Current.MainPage.DisplayAlert("Fără internet", "Verifică conexiunea la rețea.", "OK");
            return;
        }

        var toke = await SecureStorage.GetAsync("auth_token");
        if (string.IsNullOrEmpty(toke))
        {
            await Application.Current.MainPage.DisplayAlert("Eroare", "Trebuie să fii autentificat pentru a te abona!", "OK");
            return;
        }
        string userId = GetUserIdFromToken(toke);
        if (string.IsNullOrEmpty(userId))
        {
            await Application.Current.MainPage.DisplayAlert("Eroare", "Nu s-a putut obține ID-ul utilizatorului!", "OK");
            return;
        }

        var newsItem = News.FirstOrDefault(n => n.NewsId == newsId);
        if (newsItem == null) return;

        var success = await _newsService.SubscribeUnsubscribeAsyc(userId,newsItem ,toke);

        if (success)
        {
            newsItem.HasSubscribed = !newsItem.HasSubscribed;
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

    private async Task OpenBrowser(NewsItem news)
    {
        try
        {
            _ttsCancellationTokenSource?.Cancel();
            IsTtsControlVisible = false;
            var current = _newsListForTTS[_currentNewsIndex];
            current.IsHighlighted = false;
            var userId = GetUserIdFromToken(await SecureStorage.GetAsync("auth_token"));
            if (!string.IsNullOrEmpty(userId))
            {
                await _httpClient.PostAsJsonAsync($"{AppConfig.ApiBaseUrl}/news/{news.NewsId}/view",userId);
            }

            if (!string.IsNullOrWhiteSpace(news.Url))
            {
                await Application.Current.MainPage.Navigation.PushAsync(new WebViewPage(news));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Eroare la deschiderea browserului: {ex.Message}");
        }
    }

    protected virtual async Task<List<NewsItem>> FetchNews(string userId, int page)
    {
        return await _newsService.GetNewsAsync(userId, Category, page, 20, await SecureStorage.GetAsync("auth_token"));
    }
}
