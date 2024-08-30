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

            var token = _httpContextAccessor.HttpContext?.Request.Cookies["Token"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(Request.Cookies["UserId"]);
            var url = _configuration["MicroServiceUrls:UserServiceUrl"];

            try
            {
                var response = await _httpClient.GetAsync(url + "api/Users/" + userId);
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

            }

            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = int.Parse(Request.Cookies["UserId"]);
            var url = _configuration["MicroServiceUrls:UserServiceUrl"];

            try
            {
                var response = await _httpClient.GetAsync(url + "api/Users/" + userId);
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

            var url = _configuration["MicroServiceUrls:UserServiceUrl"];

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(userDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(url + "api/Users/" + userDto.UserId, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("Index", "Login");
        }
    }
}
