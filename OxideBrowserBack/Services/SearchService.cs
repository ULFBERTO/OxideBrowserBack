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
            // If the query is a URL AND it has not been indexed yet, crawl it.
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute) && !_indexService.UrlExists(query))
            {
                await _crawlerService.StartRecursiveCrawlAsync(query, maxPages ?? 10);
            }

            // Now, determine the actual search term.
            string searchTerm = query;
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                // If the original query was a URL, extract keywords from it for the search.
                try
                {
                    var host = new Uri(query).Host;
                    var parts = host.Split('.');
                    if (parts.Length >= 2)
                    {
                        // Get the part before the TLD (e.g., "youtube" from "www.youtube.com")
                        searchTerm = parts[parts.Length - 2];
                    }
                }
                catch { /* Fallback to original query if parsing fails */ }
            }

            // Perform a standard text search with the determined search term.
            return _indexService.Search(searchTerm);
        }

        public IEnumerable<IndexedPage> GetAll()
        {
            return _indexService.GetAll();
        }
    }
}
