using FrontendService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrontendService.Controllers
{
    public class PostsController : Controller
    {
        private readonly IBackendServiceClient _backendServiceClient;

        public PostsController(IBackendServiceClient backendServiceClient)
        {
            _backendServiceClient = backendServiceClient;
        }

        public async Task<IActionResult> Index()
        {
            var allPosts = await _backendServiceClient.GetDataFromService1Async();

            var sortedPosts = allPosts
            .OrderByDescending(post => post.PublishDate)
            .ToList();

            return View(sortedPosts);
        }
    }

}
