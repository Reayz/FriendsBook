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
        private readonly IConfiguration _configuration;

        public BackendServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }


        public async Task<List<PostDTO>> GetDataFromService1Async()
        {
            var url = _configuration["MicroServiceUrls:PostServiceUrl"];
            var response = await _httpClient.GetAsync(url + "api/Posts");
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
