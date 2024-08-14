using FrontendService.Models;
using System.Text.Json;

namespace FrontendService.Services
{
    public interface IBackendServiceClient
    {
        Task<List<PostDTO>> GetDataFromService1Async();
    }

    public class BackendServiceClient : IBackendServiceClient
    {
        private readonly HttpClient _httpClient;

        public BackendServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PostDTO>> GetDataFromService1Async()
        {
            var response = await _httpClient.GetAsync("https://localhost:44385/api/Posts");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<List<PostDTO>>(content, options);

            return data;
        }
    }
}
