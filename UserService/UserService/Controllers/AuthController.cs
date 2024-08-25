using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
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
                return Unauthorized();
            }

            return Ok(new { Token = token });
        }

        [HttpPost("verify-token")]
        public IActionResult VerifyToken(string token)
        {
            var claimsPrincipal = _authService.ValidateJwtToken(token);

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
