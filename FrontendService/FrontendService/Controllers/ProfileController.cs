using FrontendService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FrontendService.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileController(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = int.Parse(Request.Cookies["UserId"]);
                var response = await _httpClient.GetAsync("api/Users/" + userId);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var user = JsonSerializer.Deserialize<UserDTO>(content, options);

                    return View(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            try
            {
                var userId = int.Parse(Request.Cookies["UserId"]);
                var response = await _httpClient.GetAsync("api/Users/" + userId);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var user = JsonSerializer.Deserialize<UserDTO>(content, options);

                    return View(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                return View(userDto);
            }

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(userDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync("api/Users/" + userDto.UserId, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return RedirectToAction("Index", "Login");
        }
    }
}
