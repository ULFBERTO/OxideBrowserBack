using HtmlAgilityPack;
using OxideBrowserBack.Models;
using System.Collections.Concurrent;

namespace OxideBrowserBack.Services
{
    public class CrawlerService
    {
        private readonly HttpClient _httpClient;
        private readonly IndexService _indexService;

        public CrawlerService(IndexService indexService)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _indexService = indexService;
        }

        public async Task<int> StartRecursiveCrawlAsync(string startUrl, int maxPages = 10)
        {
            var queue = new ConcurrentQueue<string>();
            var visitedUrls = new ConcurrentDictionary<string, bool>();
            var pagesCrawled = 0;

            queue.Enqueue(startUrl);

            while (!queue.IsEmpty && pagesCrawled < maxPages)
            {
                if (!queue.TryDequeue(out var currentUrl))
                {
                    continue;
                }

                // Basic validation and normalization
                try
                {
                    var uri = new Uri(new Uri(startUrl), currentUrl);
                    if (uri.Scheme != "http" && uri.Scheme != "https" || visitedUrls.ContainsKey(uri.AbsoluteUri))
                    {
                        continue;
                    }
                    currentUrl = uri.AbsoluteUri;
                }
                catch
                {
                    continue; // Invalid URL format
                }

                if (!visitedUrls.TryAdd(currentUrl, true))
                {
                    continue;
                }

                try
                {
                    var crawledPage = await CrawlPageAsync(currentUrl);
                    if (crawledPage != null)
                    {
                        _indexService.AddPageToIndex(crawledPage);
                        pagesCrawled++;

                        foreach (var link in crawledPage.Links)
                        {
                            queue.Enqueue(link);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log errors for specific URLs but continue crawling others
                    Console.WriteLine($"Failed to crawl {currentUrl}: {ex.Message}");
                }
            }

            if (pagesCrawled > 0)
            {
                // _indexService.SaveChanges(); // No longer needed, LiteDB saves automatically
            }

            return pagesCrawled;
        }

        private async Task<CrawledPage?> CrawlPageAsync(string url)
        {
            var response = await _httpClient.GetStringAsync(url);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText ?? "No Title";
            var text = htmlDoc.DocumentNode.SelectSingleNode("//body")?.InnerText ?? "";

            var links = htmlDoc.DocumentNode.SelectNodes("//a[@href]")
                            ?.Select(node => node.GetAttributeValue("href", string.Empty))
                            .Where(link => !string.IsNullOrEmpty(link))
                            .ToList() ?? new List<string>();

            return new CrawledPage { Url = url, Title = title, Content = text, Links = links };
        }
    }

    public class CrawledPage
    {
        public required string Url { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required List<string> Links { get; set; }
    }
}
