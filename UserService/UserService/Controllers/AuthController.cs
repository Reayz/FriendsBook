using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login loginModel)
        {
            if (loginModel == null || string.IsNullOrEmpty(loginModel.UserName) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("Invalid client request");
            }

            var token = await _authService.AuthenticateAsync(loginModel.UserName, loginModel.Password);

            if (token == null)
            {
                _logger.LogInformation($"Authentication is not succesfull in the Login method.");
                return Unauthorized();
            }

            _logger.LogInformation($"Authentication is succesfull in the Login method.");
            return Ok(new { Token = token });
        }

        [HttpPost("verify-token")]
        public IActionResult VerifyToken(string token)
        {
            var claimsPrincipal = _authService.ValidateJwtToken(token);
            _logger.LogInformation($"In the VerifyToken method: {claimsPrincipal?.Identity?.Name}.");

            if (claimsPrincipal == null)
            {
                return Unauthorized();
            }

            return Ok(new { message = "Token is valid", user = claimsPrincipal.Identity.Name });
        }

        [HttpPost("logout")]
        public IActionResult Logout(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is empty.");
            }

            bool success = _authService.Logout(token);

            if (success)
            {
                return Ok("Logout successful.");
            }

            return BadRequest("Failed to logout.");
        }
    }
}
