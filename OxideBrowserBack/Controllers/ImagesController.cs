using Microsoft.AspNetCore.Mvc;
using OxideBrowserBack.Models;
using OxideBrowserBack.Services;

namespace OxideBrowserBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ContentService _contentService;

        public ImagesController(ContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchImages([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return BadRequest("Query parameter 'q' is required.");
            }

            var results = await _contentService.SearchImagesAsync(q);
            return Ok(results);
        }
    }
}
