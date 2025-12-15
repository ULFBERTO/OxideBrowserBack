using HtmlAgilityPack;
using OxideBrowserBack.Models;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace OxideBrowserBack.Services
{
    public class CrawlerService
    {
        private readonly HttpClient _httpClient;
        private readonly IndexService _indexService;

        public CrawlerService(IndexService indexService)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5
            };
            _httpClient = new HttpClient(handler);
            
            // More realistic browser headers to avoid 403s
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9,es;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
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
                    continue;

                try
                {
                    var uri = new Uri(new Uri(startUrl), currentUrl);
                    if (uri.Scheme != "http" && uri.Scheme != "https" || visitedUrls.ContainsKey(uri.AbsoluteUri))
                        continue;
                    currentUrl = uri.AbsoluteUri;
                }
                catch
                {
                    continue;
                }

                if (!visitedUrls.TryAdd(currentUrl, true))
                    continue;

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
                    Console.WriteLine($"Failed to crawl {currentUrl}: {ex.Message}");
                }
            }

            return pagesCrawled;
        }

        /// <summary>
        /// Re-crawl a specific URL and update its index
        /// </summary>
        public async Task<bool> RecrawlPageAsync(string url)
        {
            try
            {
                var crawledPage = await CrawlPageAsync(url);
                if (crawledPage != null)
                {
                    _indexService.AddPageToIndex(crawledPage);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Re-crawl multiple URLs
        /// </summary>
        public async Task<CrawlResult> RecrawlPagesAsync(IEnumerable<string> urls, IProgress<CrawlProgress>? progress = null)
        {
            var result = new CrawlResult();
            var urlList = urls.ToList();
            result.TotalUrls = urlList.Count;

            int processed = 0;
            foreach (var url in urlList)
            {
                try
                {
                    var success = await RecrawlPageAsync(url);
                    if (success)
                        result.SuccessCount++;
                    else
                        result.FailedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"{url}: {ex.Message}");
                }

                processed++;
                progress?.Report(new CrawlProgress
                {
                    Processed = processed,
                    Total = result.TotalUrls,
                    CurrentUrl = url
                });
            }

            return result;
        }

        private async Task<CrawledPage?> CrawlPageAsync(string url)
        {
            var response = await _httpClient.GetStringAsync(url);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            // Extract title
            var title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim() ?? "No Title";
            title = Regex.Replace(title, @"\s+", " "); // Normalize whitespace

            // Extract meta description
            var metaDescription = htmlDoc.DocumentNode
                .SelectSingleNode("//meta[@name='description']")
                ?.GetAttributeValue("content", string.Empty)
                ?? htmlDoc.DocumentNode
                .SelectSingleNode("//meta[@property='og:description']")
                ?.GetAttributeValue("content", string.Empty);

            // Extract meta keywords
            var metaKeywords = htmlDoc.DocumentNode
                .SelectSingleNode("//meta[@name='keywords']")
                ?.GetAttributeValue("content", string.Empty);

            // Extract body text (cleaned)
            var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
            var text = "";
            if (bodyNode != null)
            {
                // Remove script and style elements
                foreach (var script in bodyNode.SelectNodes("//script|//style|//noscript") ?? Enumerable.Empty<HtmlNode>())
                {
                    script.Remove();
                }
                text = bodyNode.InnerText;
                text = Regex.Replace(text, @"\s+", " ").Trim(); // Normalize whitespace
            }

            // Extract links
            var links = htmlDoc.DocumentNode.SelectNodes("//a[@href]")
                ?.Select(node => node.GetAttributeValue("href", string.Empty))
                .Where(link => !string.IsNullOrEmpty(link) && !link.StartsWith("#") && !link.StartsWith("javascript:"))
                .Distinct()
                .Take(100) // Limit links per page
                .ToList() ?? new List<string>();

            return new CrawledPage 
            { 
                Url = url, 
                Title = title, 
                Content = text, 
                Links = links,
                MetaDescription = metaDescription,
                MetaKeywords = metaKeywords
            };
        }
    }

    public class CrawledPage
    {
        public required string Url { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required List<string> Links { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }

    public class CrawlResult
    {
        public int TotalUrls { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class CrawlProgress
    {
        public int Processed { get; set; }
        public int Total { get; set; }
        public string? CurrentUrl { get; set; }
    }
}
