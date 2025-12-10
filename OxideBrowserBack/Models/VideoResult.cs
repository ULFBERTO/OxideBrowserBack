namespace OxideBrowserBack.Models
{
    public class VideoResult
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string ThumbnailUrl { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Channel { get; set; }
        public required string Duration { get; set; }
        public required string Views { get; set; }
        public required string PublishedAt { get; set; }
    }
}
