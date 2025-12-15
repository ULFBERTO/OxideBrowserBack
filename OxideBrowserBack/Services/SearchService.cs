using OxideBrowserBack.Models;

namespace OxideBrowserBack.Services
{
    public class SearchService
    {
        private readonly IndexService _indexService;
        private readonly CrawlerService _crawlerService;

        // Domain extensions to try
        private static readonly string[] DomainExtensions = {
            ".com", ".es", ".co", ".org", ".net", ".io", ".app", ".dev", 
            ".mx", ".ar", ".cl", ".pe", ".co.uk", ".de", ".fr", ".it"
        };

        // URL patterns to try (prioritized)
        private static readonly string[] UrlPatterns = {
            "https://www.{0}{1}",
            "https://{0}{1}",
            "https://www.{0}.info"
        };

        // Knowledge sources (high priority for people/topics)
        private static readonly string[] KnowledgeSources = {
            "https://es.wikipedia.org/wiki/{0}",
            "https://en.wikipedia.org/wiki/{0}",
            "https://www.wikidata.org/wiki/{0}",
            "https://www.imdb.com/find?q={0}",
            "https://www.last.fm/search?q={0}",
            "https://www.allmusic.com/search/all/{0}"
        };

        // News/entertainment sources
        private static readonly string[] NewsSources = {
            "https://www.billboard.com/search/{0}",
            "https://elpais.com/buscador/?qt={0}",
            "https://www.bbc.com/search?q={0}"
        };

        private const int MaxCrawlIterations = 50;

        public SearchService(IndexService indexService, CrawlerService crawlerService)
        {
            _indexService = indexService;
            _crawlerService = crawlerService;
        }

        public async Task<SearchResponse> ProcessSmartSearchAsync(string query, int? maxPages, bool allowAdultContent = false)
        {
            var response = new SearchResponse { Query = query };
            var maxPagesPerSite = maxPages ?? 10;

            // 1. Determine the effective search term
            string searchTerm = query;
            bool isUrl = Uri.IsWellFormedUriString(query, UriKind.Absolute);

            if (isUrl)
            {
                try
                {
                    var host = new Uri(query).Host;
                    var parts = host.Split('.');
                    if (parts.Length >= 2)
                    {
                        searchTerm = parts[parts.Length - 2];
                    }
                }
                catch { }
            }

            // 2. Perform initial search with adult content filtering
            var results = _indexService.Search(searchTerm, allowAdultContent).ToList();
            
            if (results.Any())
            {
                response.Results = results;
                response.TotalResults = results.Count;
                response.FromCache = true;
                return response;
            }

            // 3. If no results found, attempt aggressive crawling
            int totalCrawled = 0;
            int iterations = 0;
            var triedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var sanitizedQuery = SanitizeForUrl(query);

            if (isUrl)
            {
                // If it's a URL, try it first
                totalCrawled += await TryCrawlUrl(query, maxPagesPerSite, triedUrls);
                iterations++;
            }

            // Try different domain extensions
            foreach (var extension in DomainExtensions)
            {
                if (iterations >= MaxCrawlIterations) break;
                
                foreach (var pattern in UrlPatterns.Take(2)) // Main patterns
                {
                    if (iterations >= MaxCrawlIterations) break;
                    
                    var url = string.Format(pattern, sanitizedQuery, extension);
                    var crawled = await TryCrawlUrl(url, maxPagesPerSite, triedUrls);
                    totalCrawled += crawled;
                    iterations++;

                    // Check if we got results
                    if (crawled > 0)
                    {
                        results = _indexService.Search(searchTerm, allowAdultContent).ToList();
                        if (results.Any())
                        {
                            response.Results = results;
                            response.TotalResults = results.Count;
                            response.PagesCrawled = totalCrawled;
                            response.UrlsAttempted = iterations;
                            response.FromCache = false;
                            return response;
                        }
                    }
                }
            }

            // Try knowledge sources (Wikipedia, IMDB, etc.) - HIGH PRIORITY for people/topics
            var encodedQuery = Uri.EscapeDataString(query.Replace(" ", "_"));
            var encodedQuerySpaces = Uri.EscapeDataString(query);
            
            foreach (var pattern in KnowledgeSources)
            {
                if (iterations >= MaxCrawlIterations) break;
                
                var url = pattern.Contains("wikipedia") || pattern.Contains("wikidata")
                    ? string.Format(pattern, encodedQuery)
                    : string.Format(pattern, encodedQuerySpaces);
                    
                var crawled = await TryCrawlUrl(url, maxPagesPerSite, triedUrls);
                totalCrawled += crawled;
                iterations++;

                if (crawled > 0)
                {
                    results = _indexService.Search(searchTerm, allowAdultContent).ToList();
                    if (results.Any())
                    {
                        response.Results = results;
                        response.TotalResults = results.Count;
                        response.PagesCrawled = totalCrawled;
                        response.UrlsAttempted = iterations;
                        response.FromCache = false;
                        return response;
                    }
                }
            }

            // Try news/entertainment sources
            foreach (var pattern in NewsSources)
            {
                if (iterations >= MaxCrawlIterations) break;
                
                var url = string.Format(pattern, encodedQuerySpaces);
                var crawled = await TryCrawlUrl(url, 5, triedUrls);
                totalCrawled += crawled;
                iterations++;

                if (crawled > 0)
                {
                    results = _indexService.Search(searchTerm, allowAdultContent).ToList();
                    if (results.Any())
                    {
                        response.Results = results;
                        response.TotalResults = results.Count;
                        response.PagesCrawled = totalCrawled;
                        response.UrlsAttempted = iterations;
                        response.FromCache = false;
                        return response;
                    }
                }
            }

            // Final search attempt
            results = _indexService.Search(searchTerm, allowAdultContent).ToList();
            response.Results = results;
            response.TotalResults = results.Count;
            response.PagesCrawled = totalCrawled;
            response.UrlsAttempted = iterations;
            response.FromCache = false;

            return response;
        }

        private async Task<int> TryCrawlUrl(string url, int maxPages, HashSet<string> triedUrls)
        {
            if (triedUrls.Contains(url)) return 0;
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) return 0;

            triedUrls.Add(url);

            try
            {
                return await _crawlerService.StartRecursiveCrawlAsync(url, maxPages);
            }
            catch
            {
                return 0;
            }
        }

        private static string SanitizeForUrl(string query)
        {
            return query.Trim()
                .ToLower()
                .Replace(" ", "")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n");
        }

        /// <summary>
        /// Simple search without auto-crawling
        /// </summary>
        public IEnumerable<IndexedPage> Search(string query, bool allowAdultContent = false)
        {
            return _indexService.Search(query, allowAdultContent);
        }

        /// <summary>
        /// Reindex all pages in the database
        /// </summary>
        public async Task<ReindexResult> ReindexAllAsync(IProgress<ReindexProgress>? progress = null)
        {
            return await _indexService.ReindexAllPagesAsync(progress);
        }

        /// <summary>
        /// Reindex a specific page
        /// </summary>
        public bool ReindexPage(string url)
        {
            return _indexService.ReindexPage(url);
        }

        /// <summary>
        /// Re-crawl and reindex all pages
        /// </summary>
        public async Task<CrawlResult> RecrawlAllPagesAsync(IProgress<CrawlProgress>? progress = null)
        {
            var allPages = _indexService.GetAll().ToList();
            var urls = allPages.Select(p => p.Url);
            return await _crawlerService.RecrawlPagesAsync(urls, progress);
        }

        /// <summary>
        /// Re-crawl specific pages
        /// </summary>
        public async Task<CrawlResult> RecrawlPagesAsync(IEnumerable<string> urls, IProgress<CrawlProgress>? progress = null)
        {
            return await _crawlerService.RecrawlPagesAsync(urls, progress);
        }

        public IEnumerable<IndexedPage> GetAll()
        {
            return _indexService.GetAll();
        }

        public IndexStats GetStats()
        {
            return _indexService.GetStats();
        }

        public ClearIndexResult ClearIndex()
        {
            return _indexService.ClearAll();
        }

        public ClearIndexResult ClearByDomain(string domain)
        {
            return _indexService.ClearByDomain(domain);
        }
    }

    public class SearchResponse
    {
        public string Query { get; set; } = string.Empty;
        public List<IndexedPage> Results { get; set; } = new();
        public int TotalResults { get; set; }
        public bool FromCache { get; set; }
        public int PagesCrawled { get; set; }
        public int UrlsAttempted { get; set; }
    }
}
