using FrontendService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FrontendService.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;


        public LoginController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var loginData = new
                {
                    Username = model.Username,
                    Password = model.Password,
                };

                var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

                var url = _configuration["MicroServiceUrls:UserServiceUrl"];

                var response = await _httpClient.PostAsync(url + "api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var token = JsonSerializer.Deserialize<TokenResponse>(responseData, options);
                    if (token != null)
                    {
                        Response.Cookies.Append("Token", token.Token, new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });

                        string tempToken = token.Token;

                        Response.Cookies.Append("UserName", model.Username, new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Verify()
        {
            var token = Request.Cookies["Token"];

            var url = _configuration["MicroServiceUrls:UserServiceUrl"];
            var requestUrl = $"{url}api/Auth/verify-token?token={Uri.EscapeDataString(token)}";

            Console.WriteLine("Request URL: " + requestUrl);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Status Code: " + response.StatusCode);
                Console.WriteLine("Response Body: " + responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Home"); // Token is valid
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return RedirectToAction("Index"); // Token is not valid
        }

        public async Task<IActionResult> Logout()
        {
            var token = Request.Cookies["Token"];

            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            var url = _configuration["MicroServiceUrls:UserServiceUrl"];
            var requestUrl = $"{url}api/Auth/logout?token={Uri.EscapeDataString(token)}";

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Status Code: " + response.StatusCode);
                Console.WriteLine("Response Body: " + responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Login"); // Token is valid
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return RedirectToAction("Index"); // Token is not valid
        }
    }
}
