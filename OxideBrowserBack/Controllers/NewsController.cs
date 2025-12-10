using Microsoft.AspNetCore.Mvc;
using OxideBrowserBack.Models;
using OxideBrowserBack.Services;

namespace OxideBrowserBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly ContentService _contentService;

        public NewsController(ContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchNews([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return BadRequest("Query parameter 'q' is required.");
            }

            var results = await _contentService.SearchNewsAsync(q);
            return Ok(results);
        }
    }
}
