namespace OxideBrowserBack.Models
{
    public class TermFrequency
    {
        public int Id { get; set; }
        public string Term { get; set; } = string.Empty;
        public int PageId { get; set; }
        public int Frequency { get; set; }
        
        // Position-based scoring
        public bool InTitle { get; set; } = false;
        public bool InUrl { get; set; } = false;
        public bool InMetaDescription { get; set; } = false;
        public int FirstPosition { get; set; } = -1; // Position of first occurrence
    }
    
    // N-gram support for phrase matching
    public class NGramFrequency
    {
        public int Id { get; set; }
        public string NGram { get; set; } = string.Empty; // 2-3 word phrases
        public int PageId { get; set; }
        public int Frequency { get; set; }
        public int NGramSize { get; set; } = 2; // bigram or trigram
    }
}