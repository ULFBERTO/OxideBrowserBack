namespace OxideBrowserBack.Models
{
    public class EventResult
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Location { get; set; }
        public required string Day { get; set; }
        public required string Month { get; set; }
        public required string Time { get; set; }
        public required string Url { get; set; }
    }
}
