using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NewsFlow.Models;
using Plugin.LocalNotification;
using NewsFlow.Services;


namespace NewsFlow.ViewModels
{
    class SubscribedNewsViewModel : NewsViewModel
    {
        private readonly HttpClient _httpClient = new();
        public SubscribedNewsViewModel() : base("subscribed") { }

        protected override async Task<List<NewsItem>> FetchNews(string userId, int page)
        {
           
            var token = await SecureStorage.GetAsync("auth_token");

            var url = $"{AppConfig.ApiBaseUrl}/news/subscriptions?userId={userId}&page={page}&pageSize=20";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new List<NewsItem>();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<NewsItem>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


           

            return result ?? new List<NewsItem>();
        }
    }
    
    
}
