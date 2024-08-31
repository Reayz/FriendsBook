using FrontendService.Models;
using FrontendService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrontendService.Controllers
{
    public class PostController : Controller
    {
        private readonly ILogger<PostController> _logger;
        private readonly IBackendServiceClient _postServiceClient;

        public PostController(ILogger<PostController> logger, IBackendServiceClient backendServiceClient)
        {
            _logger = logger;
            _postServiceClient = backendServiceClient;
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["Token"];

            var allPosts = await _postServiceClient.GetAllPostsFromPostService(token);
            if (allPosts != null)
            {
                var userId = int.Parse(Request.Cookies["UserId"]);

                var sortedPosts = allPosts.Where(x => x.AuthorId == userId).OrderByDescending(post => post.PublishDate).ToList();

                return View(sortedPosts);
            }
            else
            {
                _logger.LogInformation("Getting Posts from Post service is not successfull. Redirecting to the Login page.");
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpGet]
        [Route("Post/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Post/Create")]
        public async Task<IActionResult> Create(PostDTO postDTO)
        {
            if (string.IsNullOrEmpty(postDTO.Title))
            {
                return View();
            }

            postDTO.AuthorId = int.Parse(Request.Cookies["UserId"]);
            postDTO.AuthorName = Request.Cookies["UserName"];
            postDTO.PublishDate = DateTime.UtcNow;

            var post = await _postServiceClient.CreatePost(postDTO);

            if (post.PostId > 0)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        [Route("Post/Edit/{postId}")]
        public async Task<IActionResult> Edit(int postId)
        {
            var post = await _postServiceClient.GetPostDetails(postId);

            if (post != null)
            {
                return View(post);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        [Route("Post/Edit/{postId}")]
        public async Task<IActionResult> Edit(PostDTO postDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(postDTO);
            }

            postDTO.LastEditedDate = DateTime.UtcNow;

            var success = await _postServiceClient.UpdatePostDetails(postDTO);

            if (success)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        [Route("Post/Delete/{postId}")]
        public async Task<IActionResult> Delete(int postId)
        {
            var success = await _postServiceClient.DeletePost(postId);

            if (success)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
    }
}
