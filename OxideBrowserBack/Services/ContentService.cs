using OxideBrowserBack.Models;
using System.Net.Http;
using System.Text.Json;
using HtmlAgilityPack;

namespace OxideBrowserBack.Services
{
    public class ContentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContentService> _logger;

        public ContentService(IHttpClientFactory httpClientFactory, ILogger<ContentService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "OxideBrowser/1.0");
            _logger = logger;
        }

        public async Task<List<ImageResult>> SearchImagesAsync(string query)
        {
            var images = new List<ImageResult>();
            try
            {
                var encodedQuery = Uri.EscapeDataString(query);
                var categories = new[] { "nature", "technology", "architecture", "city", "abstract", "minimal", "landscape", "portrait" };
                var sources = new[] { "Unsplash", "Pexels", "Pixabay", "StockSnap" };
                
                // Generate 40 images for pagination demo
                for (int i = 0; i < 40; i++)
                {
                    var category = categories[i % categories.Length];
                    var source = sources[i % sources.Length];
                    var seed = i + query.GetHashCode();
                    
                    // Use picsum.photos for reliable placeholder images
                    var width = 400 + (i % 3) * 100;
                    var height = 300 + (i % 4) * 100;
                    
                    images.Add(new ImageResult
                    {
                        Id = i + 1,
                        Url = $"https://picsum.photos/seed/{encodedQuery}{i}/800/600",
                        ThumbnailUrl = $"https://picsum.photos/seed/{encodedQuery}{i}/{width}/{height}",
                        Title = $"{(query.Length > 0 ? char.ToUpper(query[0]) + query.Substring(1) : "Image")} - {category} #{i + 1}",
                        Source = source,
                        Width = width,
                        Height = height
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching images for query: {Query}", query);
            }
            return images;
        }

        public async Task<List<VideoResult>> SearchVideosAsync(string query)
        {
            var videos = new List<VideoResult>();
            try
            {
                // Generate sample video results
                var channels = new[] { "TechWorld", "LearnCode", "ScienceHub", "NewsDaily", "TutorialPro", "CreativeMinds" };
                var durations = new[] { "5:32", "12:45", "8:20", "15:00", "3:45", "22:10", "7:55", "10:30" };
                var views = new[] { "1.2M", "856K", "2.3M", "445K", "3.1M", "125K", "567K", "890K" };
                var times = new[] { "2 hours ago", "1 day ago", "3 days ago", "1 week ago", "2 weeks ago", "1 month ago" };

                var encodedQuery = Uri.EscapeDataString(query);
                for (int i = 0; i < 8; i++)
                {
                    videos.Add(new VideoResult
                    {
                        Id = i + 1,
                        Url = $"https://www.youtube.com/watch?v=example{i}",
                        ThumbnailUrl = $"https://picsum.photos/seed/video{encodedQuery}{i}/640/360",
                        Title = $"{query} - Complete Guide Part {i + 1}",
                        Description = $"Learn everything about {query} in this comprehensive video tutorial.",
                        Channel = channels[i % channels.Length],
                        Duration = durations[i % durations.Length],
                        Views = views[i % views.Length],
                        PublishedAt = times[i % times.Length]
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching videos for query: {Query}", query);
            }
            return videos;
        }


        public async Task<List<NewsResult>> SearchNewsAsync(string query)
        {
            var news = new List<NewsResult>();
            try
            {
                // Generate sample news results
                var sources = new[] { "TechCrunch", "The Verge", "Wired", "BBC News", "CNN", "Reuters", "Bloomberg", "Forbes" };
                var times = new[] { "30 minutes ago", "1 hour ago", "2 hours ago", "5 hours ago", "Yesterday", "2 days ago" };

                var headlines = new[]
                {
                    $"Breaking: Major developments in {query} industry",
                    $"How {query} is changing the world in 2024",
                    $"Experts weigh in on the future of {query}",
                    $"New study reveals surprising facts about {query}",
                    $"The rise of {query}: What you need to know",
                    $"Industry leaders discuss {query} trends"
                };

                var encodedQuery = Uri.EscapeDataString(query);
                for (int i = 0; i < 6; i++)
                {
                    news.Add(new NewsResult
                    {
                        Id = i + 1,
                        Url = $"https://news.example.com/article/{i}",
                        Title = headlines[i],
                        Description = $"In-depth coverage of {query} and its impact on various sectors. Our reporters bring you the latest updates and expert analysis on this developing story.",
                        Source = sources[i % sources.Length],
                        ImageUrl = $"https://picsum.photos/seed/news{encodedQuery}{i}/400/250",
                        PublishedAt = times[i % times.Length]
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching news for query: {Query}", query);
            }
            return news;
        }

        public async Task<List<EventResult>> SearchEventsAsync(string query)
        {
            var events = new List<EventResult>();
            try
            {
                var locations = new[] { "New York, NY", "San Francisco, CA", "London, UK", "Tokyo, Japan", "Berlin, Germany", "Online" };
                var months = new[] { "DEC", "JAN", "FEB", "MAR" };
                var times = new[] { "9:00 AM - 5:00 PM", "10:00 AM - 6:00 PM", "2:00 PM - 8:00 PM", "All Day" };

                var eventTypes = new[]
                {
                    $"{query} Conference 2024",
                    $"Workshop: Introduction to {query}",
                    $"{query} Meetup & Networking",
                    $"Advanced {query} Masterclass",
                    $"{query} Summit",
                    $"Hands-on {query} Training"
                };

                for (int i = 0; i < 6; i++)
                {
                    events.Add(new EventResult
                    {
                        Id = i + 1,
                        Title = eventTypes[i],
                        Description = $"Join us for an exciting event focused on {query}. Network with professionals and learn from industry experts.",
                        Location = locations[i % locations.Length],
                        Day = ((i * 5 + 10) % 28 + 1).ToString(),
                        Month = months[i % months.Length],
                        Time = times[i % times.Length],
                        Url = $"https://events.example.com/event/{i}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching events for query: {Query}", query);
            }
            return events;
        }
    }
}
