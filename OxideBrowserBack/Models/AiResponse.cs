namespace OxideBrowserBack.Models
{
    public class AiResponse
    {
        public required string Response { get; set; }
        public List<string> Sources { get; set; } = new();
    }

    public class AiRequest
    {
        public string? Query { get; set; }
    }
}
