using Microsoft.AspNetCore.Mvc;
using OxideBrowserBack.Services;

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
        /// GET /api/search?p=query&allowAdult=false
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string? p, 
            [FromQuery] int? maxPages,
            [FromQuery] bool allowAdult = false)
        {
            if (string.IsNullOrEmpty(p))
            {
                return BadRequest("Query parameter 'p' is required.");
            }

            var response = await _searchService.ProcessSmartSearchAsync(p, maxPages ?? 10, allowAdult);
            return Ok(response);
        }

        /// <summary>
        /// Búsqueda POST con más opciones
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SmartSearch([FromBody] SearchRequest request)
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return BadRequest("Query parameter is required.");
            }

            var response = await _searchService.ProcessSmartSearchAsync(
                request.Query, 
                request.MaxPages, 
                request.AllowAdultContent);
            return Ok(response);
        }

        /// <summary>
        /// Búsqueda simple sin auto-crawling
        /// GET /api/search/quick?p=query
        /// </summary>
        [HttpGet("quick")]
        public IActionResult QuickSearch(
            [FromQuery] string? p,
            [FromQuery] bool allowAdult = false)
        {
            if (string.IsNullOrEmpty(p))
            {
                return BadRequest("Query parameter 'p' is required.");
            }

            var results = _searchService.Search(p, allowAdult);
            return Ok(results);
        }

        /// <summary>
        /// Obtener todas las páginas indexadas
        /// </summary>
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var results = _searchService.GetAll();
            return Ok(results);
        }

        /// <summary>
        /// Obtener estadísticas del índice
        /// </summary>
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var stats = _searchService.GetStats();
            return Ok(stats);
        }

        /// <summary>
        /// Reindexar todas las páginas existentes (recalcula rankings, scores, etc.)
        /// POST /api/search/reindex
        /// </summary>
        [HttpPost("reindex")]
        public async Task<IActionResult> ReindexAll()
        {
            var result = await _searchService.ReindexAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Reindexar una página específica por URL
        /// POST /api/search/reindex/page
        /// </summary>
        [HttpPost("reindex/page")]
        public IActionResult ReindexPage([FromBody] ReindexPageRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
            {
                return BadRequest("URL is required.");
            }

            var success = _searchService.ReindexPage(request.Url);
            if (success)
            {
                return Ok(new { message = "Page reindexed successfully", url = request.Url });
            }
            return NotFound(new { message = "Page not found in index", url = request.Url });
        }

        /// <summary>
        /// Re-crawlear y reindexar todas las páginas (actualiza contenido desde la web)
        /// POST /api/search/recrawl
        /// </summary>
        [HttpPost("recrawl")]
        public async Task<IActionResult> RecrawlAll()
        {
            var result = await _searchService.RecrawlAllPagesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Re-crawlear páginas específicas
        /// POST /api/search/recrawl/pages
        /// </summary>
        [HttpPost("recrawl/pages")]
        public async Task<IActionResult> RecrawlPages([FromBody] RecrawlPagesRequest request)
        {
            if (request.Urls == null || !request.Urls.Any())
            {
                return BadRequest("At least one URL is required.");
            }

            var result = await _searchService.RecrawlPagesAsync(request.Urls);
            return Ok(result);
        }

        /// <summary>
        /// Limpiar todo el índice (eliminar todas las páginas)
        /// DELETE /api/search/clear
        /// </summary>
        [HttpDelete("clear")]
        public IActionResult ClearIndex()
        {
            var result = _searchService.ClearIndex();
            return Ok(result);
        }

        /// <summary>
        /// Eliminar páginas por dominio
        /// DELETE /api/search/clear/domain?domain=example.com
        /// </summary>
        [HttpDelete("clear/domain")]
        public IActionResult ClearByDomain([FromQuery] string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return BadRequest("Domain parameter is required.");
            }

            var result = _searchService.ClearByDomain(domain);
            return Ok(result);
        }
    }

    public class SearchRequest
    {
        public string? Query { get; set; }
        public int? MaxPages { get; set; }
        public bool AllowAdultContent { get; set; } = false;
    }

    public class ReindexPageRequest
    {
        public string? Url { get; set; }
    }

    public class RecrawlPagesRequest
    {
        public List<string>? Urls { get; set; }
    }
}