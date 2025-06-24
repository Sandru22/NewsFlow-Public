using NewsFlow.Models;
using Plugin.LocalNotification;
using Shiny.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewsFlow.Services
{
    public class CheckSubscribedNewsJob : IJob
    {
        private readonly HttpClient _httpClient = new();

        public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                    return;

                var userId = GetUserIdFromToken(token);
                if (string.IsNullOrEmpty(userId))
                    return;

                var url = $"{AppConfig.ApiBaseUrl}/news/subscriptions?userId={userId}&page=1&pageSize=10";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.SendAsync(request, cancelToken);

                if (!response.IsSuccessStatusCode)
                    return;

                var content = await response.Content.ReadAsStringAsync();
                var newsList = JsonSerializer.Deserialize<List<NewsItem>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (newsList == null || !newsList.Any())
                    return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JobError] {ex.Message}");
            }
        }

        private string GetUserIdFromToken(string token)
        {
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                return jwtToken?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
