namespace OxideBrowserBack.Models
{
    public class NewsResult
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Source { get; set; }
        public string? ImageUrl { get; set; }
        public required string PublishedAt { get; set; }
    }
}
