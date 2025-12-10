using Microsoft.AspNetCore.Mvc;
using OxideBrowserBack.Models;
using OxideBrowserBack.Services;

namespace OxideBrowserBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly ContentService _contentService;

        public VideosController(ContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchVideos([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return BadRequest("Query parameter 'q' is required.");
            }

            var results = await _contentService.SearchVideosAsync(q);
            return Ok(results);
        }
    }
}
