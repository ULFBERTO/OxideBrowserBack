using OxideBrowserBack.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxideBrowserBack.Services
{
    public class SearchService
    {
        private readonly IndexService _indexService;
        private readonly CrawlerService _crawlerService;

        public SearchService(IndexService indexService, CrawlerService crawlerService)
        {
            _indexService = indexService;
            _crawlerService = crawlerService;
        }

        public async Task<IEnumerable<IndexedPage>> ProcessSmartSearchAsync(string query, int? maxPages)
        {
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
                catch { /* Keep original query as search term */ }
            }

            // 2. Perform initial search
            var results = _indexService.Search(searchTerm);
            if (results.Any())
            {
                return results;
            }

            // 3. If no results found, attempt to crawl
            string urlToCrawl = null;

            if (isUrl)
            {
                urlToCrawl = query;
            }
            else
            {
                // Fallback: Construct a URL from the query (e.g. "Hola" -> "https://www.hola.com")
                var sanitized = query.Trim().Replace(" ", "");
                urlToCrawl = $"https://www.{sanitized}.com";
            }

            if (!string.IsNullOrEmpty(urlToCrawl) && Uri.IsWellFormedUriString(urlToCrawl, UriKind.Absolute))
            {
                // Crawl the constructed or provided URL
                await _crawlerService.StartRecursiveCrawlAsync(urlToCrawl, maxPages ?? 10);

                // 4. Search again after crawling
                return _indexService.Search(searchTerm);
            }

            return Enumerable.Empty<IndexedPage>();
        }

        public IEnumerable<IndexedPage> GetAll()
        {
            return _indexService.GetAll();
        }
    }
}
