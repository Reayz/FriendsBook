using FrontendService.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FrontendService.Services
{
    public interface IBackendServiceClient
    {
        Task<List<PostDTO>> GetAllPostsFromPostService(string token);
        Task<PostDTO> GetPostDetails(int postId);
        Task<bool> UpdatePostDetails(PostDTO post);
        Task<bool> DeletePost(int postId);
        Task<PostDTO> CreatePost(PostDTO post);
    }

    public class BackendServiceClient : IBackendServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackendServiceClient(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

            _httpClient.BaseAddress = new Uri(_configuration["ApplicationUrl"]);
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["Token"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<PostDTO>> GetAllPostsFromPostService(string token)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Posts");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var data = JsonSerializer.Deserialize<List<PostDTO>>(content, options);

                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return null;
        }

        public async Task<PostDTO> GetPostDetails(int postId)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Posts/" + postId);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var data = JsonSerializer.Deserialize<PostDTO>(content, options);

                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return null;
        }

        public async Task<bool> UpdatePostDetails(PostDTO post)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(post), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("api/Posts/" + post.PostId, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return false;
        }

        public async Task<bool> DeletePost(int postId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync("api/Posts/" + postId);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return false;
        }

        public async Task<PostDTO> CreatePost(PostDTO post)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(post), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/Posts", content);

                if (response.IsSuccessStatusCode)
                {
                    var contentStr = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var data = JsonSerializer.Deserialize<PostDTO>(contentStr, options);

                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return null;
        }
    }
}
