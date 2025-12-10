namespace OxideBrowserBack.Models
{
    public class ImageResult
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string ThumbnailUrl { get; set; }
        public required string Title { get; set; }
        public required string Source { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
