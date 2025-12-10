using Microsoft.AspNetCore.Mvc;
using OxideBrowserBack.Services;
using System.Threading.Tasks;

namespace OxideBrowserBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Búsqueda GET con parámetro p (para uso desde la barra de direcciones del navegador)
        /// GET /api/search?p=query
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? p, [FromQuery] int? maxPages)
        {
            if (string.IsNullOrEmpty(p))
            {
                return BadRequest("Query parameter 'p' is required.");
            }

            var results = await _searchService.ProcessSmartSearchAsync(p, maxPages ?? 10);
            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> SmartSearch([FromBody] SearchRequest request)
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return BadRequest("Query parameter is required.");
            }

            var results = await _searchService.ProcessSmartSearchAsync(request.Query, request.MaxPages);
            return Ok(results);
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var results = _searchService.GetAll();
            return Ok(results);
        }
    }

    public class SearchRequest
    {
        public string? Query { get; set; }
        public int? MaxPages { get; set; }
    }
}