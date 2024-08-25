using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Data;

namespace UserService.Services
{
    public interface IAuthService
    {
        Task<string> AuthenticateAsync(string username, string password);
        ClaimsPrincipal ValidateJwtToken(string token);
        bool Logout(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly string _jwtSecret;
        private readonly IConfiguration _configuration;
        private readonly UserServiceContext _context;
        private static readonly HashSet<string> BlacklistedTokens = new HashSet<string>();

        public AuthService(IConfiguration configuration, UserServiceContext context)
        {
            _configuration = configuration;
            _context = context;
            _jwtSecret = _configuration["Jwt:Key"];
        }

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            if (_context.User == null)
            {
                return null;
            }

            var user = await _context.User.FirstOrDefaultAsync(x => x.UserName.ToLower() == username.ToLower() && x.Password == password);

            if (user == null)
            {
                return null;
            }

            // Generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal ValidateJwtToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            if (BlacklistedTokens.Contains(token))
            {
                return null; // Token is already blacklisted
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero,
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken)
                {
                    if (jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return principal;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public bool Logout(string token)
        {
            if (BlacklistedTokens.Contains(token))
            {
                return false; // Token is already blacklisted
            }

            BlacklistedTokens.Add(token);

            return true;
        }
    }
}
