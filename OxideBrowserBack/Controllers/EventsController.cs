using Microsoft.AspNetCore.Mvc;
using OxideBrowserBack.Models;
using OxideBrowserBack.Services;

namespace OxideBrowserBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ContentService _contentService;

        public EventsController(ContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchEvents([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return BadRequest("Query parameter 'q' is required.");
            }

            var results = await _contentService.SearchEventsAsync(q);
            return Ok(results);
        }
    }
}
