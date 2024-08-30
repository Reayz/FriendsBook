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

            var token = _httpContextAccessor.HttpContext?.Request.Cookies["Token"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<PostDTO>> GetAllPostsFromPostService(string token)
        {
            var url = _configuration["MicroServiceUrls:PostServiceUrl"];

            try
            {
                var response = await _httpClient.GetAsync(url + "api/Posts");
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

            }

            return null;
        }

        public async Task<PostDTO> GetPostDetails(int postId)
        {
            var url = _configuration["MicroServiceUrls:PostServiceUrl"];

            try
            {
                var response = await _httpClient.GetAsync(url + "api/Posts/" + postId);
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

            }

            return null;
        }

        public async Task<bool> UpdatePostDetails(PostDTO post)
        {
            var url = _configuration["MicroServiceUrls:PostServiceUrl"];
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(post), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url + "api/Posts/" + post.PostId, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        public async Task<bool> DeletePost(int postId)
        {
            var url = _configuration["MicroServiceUrls:PostServiceUrl"];
            try
            {
                var response = await _httpClient.DeleteAsync(url + "api/Posts/" + postId);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        public async Task<PostDTO> CreatePost(PostDTO post)
        {
            var url = _configuration["MicroServiceUrls:PostServiceUrl"];

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(post), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url + "api/Posts", content);

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

            }

            return null;
        }
    }
}
