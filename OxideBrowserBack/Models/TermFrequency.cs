namespace OxideBrowserBack.Models
{
    public class TermFrequency
    {
        public int Id { get; set; }

        public string Term { get; set; } = string.Empty;

        public int PageId { get; set; }

        public int Frequency { get; set; }
    }
}