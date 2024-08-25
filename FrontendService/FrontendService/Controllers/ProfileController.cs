using FrontendService.Models;
using FrontendService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace FrontendService.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ProfileController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }


        public async Task<IActionResult> Index()
        {
            var userName = Request.Cookies["UserName"];
            var userId = 1;

            var url = _configuration["MicroServiceUrls:UserServiceUrl"];

            var response = await _httpClient.GetAsync(url + "api/Users/" + userId);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var user = JsonSerializer.Deserialize<UserDTO>(content, options);

            return View(user);
        }
    }
}
