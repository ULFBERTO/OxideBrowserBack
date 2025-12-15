namespace OxideBrowserBack.Models
{
    public class IndexedPage
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public int WordCount { get; set; }
        
        // Ranking factors
        public double PageRank { get; set; } = 1.0;
        public int InboundLinks { get; set; } = 0;
        public int OutboundLinks { get; set; } = 0;
        public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
        
        // Content classification
        public bool IsAdultContent { get; set; } = false;
        public double AdultContentScore { get; set; } = 0.0;
        
        // Quality signals
        public double ContentQualityScore { get; set; } = 1.0;
        public int TitleLength { get; set; } = 0;
        public bool HasMetaDescription { get; set; } = false;
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        
        // Domain authority (simplified)
        public string? Domain { get; set; }
        public double DomainTrustScore { get; set; } = 1.0;
    }
}
 