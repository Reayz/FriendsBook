using FrontendService.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FrontendService.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private static string ViewDataMsg = string.Empty;

        public LoginController(ILogger<LoginController> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["ApplicationUrl"]);
        }

        public IActionResult Index()
        {
            ViewData["ErrorMsg"] = ViewDataMsg;
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
                    model.Username,
                    model.Password,
                };

                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

                    var requestUrl = $"api/Auth/login";

                    var response = await _httpClient.PostAsync(requestUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Response from Auth/UserService is success.");
                        var responseData = await response.Content.ReadAsStringAsync();

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var token = JsonSerializer.Deserialize<TokenResponse>(responseData, options);
                        if (token != null)
                        {
                            var handler = new JwtSecurityTokenHandler();
                            var jsonToken = handler.ReadToken(token.Token) as JwtSecurityToken;

                            if (jsonToken != null)
                            {
                                var claims = jsonToken.Claims;

                                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value;

                                var cookieOptions = new CookieOptions
                                {
                                    Expires = DateTimeOffset.UtcNow.AddDays(1)
                                };

                                Response.Cookies.Append("Token", token.Token, cookieOptions);
                                Response.Cookies.Append("UserName", model.Username, cookieOptions);
                                Response.Cookies.Append("UserId", userId, cookieOptions);

                                ViewDataMsg = "";
                                _logger.LogInformation("Token, UserName, UserId successfully added to the cookies.");
                                return RedirectToAction("Index", "Post");
                            }
                            else
                            {
                                _logger.LogInformation("jsonToken is null.");
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Token is null.");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Response from Auth/UserService is not success. Status code: {response.StatusCode}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }
            else
            {
                _logger.LogInformation("ModelState is not valid for login.");
            }

            ViewDataMsg = "Wrong credentials, Please try again.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Verify()
        {
            var token = Request.Cookies["Token"];

            if (token == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var requestUrl = $"api/Auth/verify-token?token={Uri.EscapeDataString(token)}";
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                var response = await _httpClient.SendAsync(request);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Status Code: " + response.StatusCode);
                Console.WriteLine("Response Body: " + responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Post"); // Token is valid
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

            if (token == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                var requestUrl = $"api/Auth/logout?token={Uri.EscapeDataString(token)}";
                var response = await _httpClient.PostAsync(requestUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();

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
