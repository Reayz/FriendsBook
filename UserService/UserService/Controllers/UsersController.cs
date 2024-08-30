using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly UserServiceContext _context;
        private readonly IAuthService _authService;

        public UsersController(ILogger<UsersController> logger, UserServiceContext context, IAuthService authService)
        {
            _logger = logger;
            _context = context;
            _authService = authService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            if (_context.User == null)
            {
                return NotFound();
            }
            return await _context.User.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            _logger.LogInformation($"In the GetUser(HttpGet) method: {token}.");

            var claimsPrincipal = _authService.ValidateJwtToken(token);

            if (claimsPrincipal == null)
            {
                return Unauthorized();
            }

            if (_context.User == null)
            {
                return NotFound();
            }
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            _logger.LogInformation($"In the PutUser(HttpPut) method: {token}.");

            var claimsPrincipal = _authService.ValidateJwtToken(token);

            if (claimsPrincipal == null)
            {
                return Unauthorized();
            }

            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'UserServiceContext.User'  is null.");
            }
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.User?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
