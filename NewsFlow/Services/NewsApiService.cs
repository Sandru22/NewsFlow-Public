using Microsoft.Maui.Controls.PlatformConfiguration;
using NewsFlow.Models;
using NewsFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewsFlow.Services
{

    public class NewsApiService
    {
        private readonly HttpClient _httpClient;

        public NewsApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<NewsItem>> GetNewsAsync(string userId, string category, int page, int pageSize = 20, string token = null)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            string url = category == "recommended"
                ? $"{AppConfig.ApiBaseUrl}/News/recommended?userId={userId}&page={page}&pageSize={pageSize}"
                : $"{AppConfig.ApiBaseUrl}/News/category/{category}?userId={userId}&page={page}&pageSize={pageSize}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<NewsItem>();

            return await JsonSerializer.DeserializeAsync<List<NewsItem>>(
                await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<NewsItem> PopulateNewsLikesAsync(NewsItem news, string userId)
        {
            var response = await _httpClient.GetAsync($"{AppConfig.ApiBaseUrl}/News/{news.NewsId}/likes?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var likesData = await JsonSerializer.DeserializeAsync<LikesData>(
                    await response.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                news.Likes = likesData?.TotalLikes ?? 0;
                news.HasLiked = likesData?.UserLiked ?? false;
            }

            return news;
        }

        public async Task<bool> ShareNewsAsync(string userId, NewsItem news, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(
                    $"{AppConfig.ApiBaseUrl}/news/{news.NewsId}/share",
                    new StringContent(JsonSerializer.Serialize(userId), Encoding.UTF8, "application/json"));

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la ShareNewsAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SubscribeUnsubscribeAsyc(string userId,NewsItem news,string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response;
                if (news.HasSubscribed)
                {
                  
                    var requestUri = $"{AppConfig.ApiBaseUrl}/news/unsubscribe/{userId}?source={news.Source}";
                    response = await _httpClient.DeleteAsync(requestUri); 
                }
                else
                {
                   
                    response = await _httpClient.PostAsync(
                        $"{AppConfig.ApiBaseUrl}/news/subscribe/{userId}?source={news.Source}",
                        new StringContent(JsonSerializer.Serialize(userId), Encoding.UTF8, "application/json"));
                }

                if (!response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Eroare", $"Operația de subscribe/unsubscribe a eșuat", "OK");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la SubscribeUnsubscribeAsyc: {ex.Message}");
                return false;
            }
        }

        public async Task LikeUnLikeAsync(string userId, NewsItem newsItem, int newsId, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response;
                if (newsItem.HasLiked)
                {
                    response = await _httpClient.PostAsync(
                        $"{AppConfig.ApiBaseUrl}/news/{newsId}/unlike",
                        new StringContent(JsonSerializer.Serialize(userId), Encoding.UTF8, "application/json"));
                }
                else
                {
                    response = await _httpClient.PostAsync(
                        $"{AppConfig.ApiBaseUrl}/news/{newsId}/like",
                        new StringContent(JsonSerializer.Serialize(userId), Encoding.UTF8, "application/json"));
                }

                if (response.IsSuccessStatusCode)
                {
                    var likesResponse = await _httpClient.GetAsync($"{AppConfig.ApiBaseUrl}/News/{newsId}/likes?userId={userId}");
                    if (likesResponse.IsSuccessStatusCode)
                    {
                        var likesData = await JsonSerializer.DeserializeAsync<LikesData>(
                            await likesResponse.Content.ReadAsStreamAsync(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (likesData != null)
                        {
                            newsItem.Likes = likesData.TotalLikes;
                            newsItem.HasLiked = likesData.UserLiked;

                        }
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Eroare", "Operația de like/unlike a eșuat", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la like/unlike: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Eroare", "A apărut o eroare la procesarea like-ului", "OK");
            }
        }


        public async Task<NewsItem> GetNewsByIdAsync(int newsId, string userId, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{AppConfig.ApiBaseUrl}/news/{newsId}?userId={userId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<NewsItem>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
