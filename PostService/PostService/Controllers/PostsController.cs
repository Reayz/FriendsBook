using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Models;

namespace PostService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ILogger<PostsController> _logger;
        private readonly PostServiceContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PostsController(ILogger<PostsController> logger, PostServiceContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPost()
        {
            var isValid = await ValidToken();
            _logger.LogWarning($"Value of isValid in the GetPost method: {isValid}.");
            if (isValid)
            {
                if (_context.Post == null)
                {
                    return NotFound();
                }

                return await _context.Post.ToListAsync();
            }

            return Unauthorized();
        }

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var isValid = await ValidToken();

            if (!isValid)
            {
                return Unauthorized();
            }

            if (_context.Post == null)
            {
                return NotFound();
            }

            var post = await _context.Post.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        // PUT: api/Posts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
            var isValid = await ValidToken();

            if (!isValid)
            {
                return Unauthorized();
            }

            if (id != post.PostId)
            {
                return BadRequest();
            }

            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
            }

            return NoContent();
        }

        // POST: api/Posts
        [HttpPost]
        public async Task<ActionResult<Post>> PostPost(Post post)
        {
            var isValid = await ValidToken();

            if (!isValid)
            {
                return Unauthorized();
            }

            if (_context.Post == null)
            {
                return Problem("Entity set 'PostServiceContext.Post'  is null.");
            }

            _context.Post.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPost", new { id = post.PostId }, post);
        }

        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var isValid = await ValidToken();

            if (!isValid)
            {
                return Unauthorized();
            }

            if (_context.Post == null)
            {
                return NotFound();
            }

            var post = await _context.Post.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            _context.Post.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return (_context.Post?.Any(e => e.PostId == id)).GetValueOrDefault();
        }

        private async Task<bool> ValidToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            _logger.LogWarning($"Token in the ValidToken method: {token}.");

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var url = _configuration["MicroServiceUrls:UserServiceUrl"];
            var requestUrl = $"{url}api/Auth/verify-token?token={Uri.EscapeDataString(token)}";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
