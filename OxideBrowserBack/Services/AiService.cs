using OxideBrowserBack.Models;

namespace OxideBrowserBack.Services
{
    public class AiService
    {
        private readonly IndexService _indexService;
        private readonly ILogger<AiService> _logger;

        public AiService(IndexService indexService, ILogger<AiService> logger)
        {
            _indexService = indexService;
            _logger = logger;
        }

        public async Task<AiResponse> GenerateResponseAsync(string query)
        {
            try
            {
                // Search for relevant content in our index
                var searchResults = _indexService.Search(query).Take(5).ToList();
                var sources = searchResults.Select(r => r.Url).ToList();

                // Generate a contextual response based on the query and search results
                var response = GenerateContextualResponse(query, searchResults);

                return new AiResponse
                {
                    Response = response,
                    Sources = sources
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI response for query: {Query}", query);
                return new AiResponse
                {
                    Response = "I apologize, but I couldn't process your request at this time. Please try again.",
                    Sources = new List<string>()
                };
            }
        }

        private string GenerateContextualResponse(string query, List<IndexedPage> results)
        {
            var queryLower = query.ToLower();

            // Knowledge base for common topics
            var knowledgeBase = new Dictionary<string, string>
            {
                { "artificial intelligence", "Artificial Intelligence (AI) refers to the simulation of human intelligence in machines programmed to think and learn. AI encompasses machine learning, neural networks, natural language processing, and computer vision. Modern AI applications include virtual assistants, autonomous vehicles, medical diagnosis, and content recommendation systems." },
                { "ai", "AI (Artificial Intelligence) is transforming industries worldwide. From healthcare diagnostics to autonomous vehicles, AI systems are becoming increasingly sophisticated. Key areas include machine learning, deep learning, natural language processing, and computer vision." },
                { "climate change", "Climate change refers to long-term shifts in global temperatures and weather patterns. Human activities, particularly burning fossil fuels, have been the main driver since the 1800s. Effects include rising sea levels, extreme weather events, and ecosystem disruption. Solutions focus on renewable energy, carbon capture, and sustainable practices." },
                { "cryptocurrency", "Cryptocurrency is a digital or virtual currency secured by cryptography. Bitcoin, created in 2009, was the first decentralized cryptocurrency. The technology relies on blockchain, a distributed ledger enforced by a network of computers. Popular cryptocurrencies include Bitcoin, Ethereum, and many altcoins." },
                { "space exploration", "Space exploration involves the discovery and exploration of celestial structures using space technology. Recent achievements include Mars rovers, the James Webb Space Telescope, and commercial spaceflight. Future goals include lunar bases, Mars colonization, and deep space exploration." },
                { "quantum computing", "Quantum computing harnesses quantum mechanical phenomena like superposition and entanglement to process information. Unlike classical computers using bits (0 or 1), quantum computers use qubits that can exist in multiple states simultaneously, enabling exponentially faster calculations for certain problems." },
                { "electric vehicles", "Electric vehicles (EVs) are automobiles powered by electric motors using energy stored in batteries. Benefits include zero direct emissions, lower operating costs, and reduced dependence on fossil fuels. Major manufacturers include Tesla, Rivian, and traditional automakers transitioning to electric." },
                { "machine learning", "Machine Learning is a subset of AI that enables systems to learn and improve from experience without explicit programming. Types include supervised learning, unsupervised learning, and reinforcement learning. Applications range from recommendation systems to fraud detection and medical diagnosis." },
                { "programming", "Programming is the process of creating instructions for computers to execute. Popular languages include Python, JavaScript, Java, C++, and Rust. Modern development involves frameworks, version control, testing, and continuous integration/deployment practices." },
                { "web development", "Web development encompasses building and maintaining websites. Frontend development focuses on user interfaces using HTML, CSS, and JavaScript frameworks like React and Angular. Backend development handles server-side logic using languages like Node.js, Python, or C#." }
            };

            // Check if query matches any knowledge base entry
            foreach (var entry in knowledgeBase)
            {
                if (queryLower.Contains(entry.Key))
                {
                    return entry.Value;
                }
            }

            // If we have search results, summarize them
            if (results.Any())
            {
                var contentSummary = string.Join(" ", results.Take(3).Select(r => r.Content.Length > 200 ? r.Content.Substring(0, 200) : r.Content));
                return $"Based on my search results for \"{query}\": {contentSummary}... I found {results.Count} relevant sources that discuss this topic in detail.";
            }

            // Default response for unknown queries
            return $"I searched for information about \"{query}\" but couldn't find specific details in my current knowledge base. This could be a specialized topic or recent development. I recommend checking authoritative sources for the most accurate and up-to-date information on this subject.";
        }
    }
}
