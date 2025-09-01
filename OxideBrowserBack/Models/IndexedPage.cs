namespace OxideBrowserBack.Models
{
    public class IndexedPage
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
    }
}
 