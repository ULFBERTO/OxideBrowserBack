using System.Text.RegularExpressions;

namespace OxideBrowserBack.Services
{
    /// <summary>
    /// Service for detecting and filtering adult/NSFW content.
    /// Content is still indexed but requires explicit search terms to appear in results.
    /// </summary>
    public class ContentFilterService
    {
        // Explicit adult terms that indicate +18 content
        private static readonly HashSet<string> AdultTerms = new(StringComparer.OrdinalIgnoreCase)
        {
            "porn", "xxx", "adult", "nsfw", "sex", "nude", "naked", "erotic",
            "hentai", "xvideos", "pornhub", "xnxx", "redtube", "youporn",
            "brazzers", "onlyfans", "camgirl", "webcam adult", "escort",
            "stripper", "fetish", "bdsm", "milf", "hardcore", "softcore"
        };

        // Terms that when combined with other words might indicate adult content
        private static readonly HashSet<string> SuspiciousTerms = new(StringComparer.OrdinalIgnoreCase)
        {
            "hot", "sexy", "girls", "babes", "teens", "mature", "amateur",
            "live", "cam", "chat", "dating", "hookup", "singles"
        };

        // Known adult domains
        private static readonly HashSet<string> AdultDomains = new(StringComparer.OrdinalIgnoreCase)
        {
            "pornhub.com", "xvideos.com", "xnxx.com", "redtube.com",
            "youporn.com", "xhamster.com", "tube8.com", "spankbang.com",
            "onlyfans.com", "chaturbate.com", "livejasmin.com"
        };

        /// <summary>
        /// Analyzes content and returns an adult content score (0.0 - 1.0)
        /// </summary>
        public (bool isAdult, double score) AnalyzeContent(string url, string title, string content)
        {
            double score = 0.0;
            int matches = 0;

            // Check domain
            try
            {
                var uri = new Uri(url);
                var domain = uri.Host.ToLower();
                if (AdultDomains.Any(d => domain.Contains(d)))
                {
                    return (true, 1.0);
                }
            }
            catch { }

            var fullText = $"{title} {content}".ToLower();
            var words = Regex.Split(fullText, @"\W+").Where(w => !string.IsNullOrEmpty(w)).ToList();

            // Count explicit adult terms
            foreach (var term in AdultTerms)
            {
                var termCount = words.Count(w => w.Equals(term, StringComparison.OrdinalIgnoreCase));
                if (termCount > 0)
                {
                    matches += termCount;
                    score += 0.15 * Math.Min(termCount, 5); // Cap contribution per term
                }
            }

            // Check for suspicious term combinations
            int suspiciousCount = 0;
            foreach (var term in SuspiciousTerms)
            {
                if (words.Contains(term, StringComparer.OrdinalIgnoreCase))
                {
                    suspiciousCount++;
                }
            }

            // Multiple suspicious terms together increase score
            if (suspiciousCount >= 3)
            {
                score += 0.1 * (suspiciousCount - 2);
            }

            // Check title specifically (higher weight)
            var titleLower = title.ToLower();
            foreach (var term in AdultTerms)
            {
                if (titleLower.Contains(term))
                {
                    score += 0.25;
                }
            }

            // Normalize score
            score = Math.Min(score, 1.0);

            // Threshold for adult classification
            bool isAdult = score >= 0.3;

            return (isAdult, score);
        }

        /// <summary>
        /// Checks if the search query explicitly requests adult content
        /// </summary>
        public bool IsExplicitAdultSearch(string query)
        {
            var queryLower = query.ToLower().Trim();
            var queryWords = Regex.Split(queryLower, @"\W+").Where(w => !string.IsNullOrEmpty(w)).ToList();

            // Must contain at least one explicit adult term
            int explicitTermCount = queryWords.Count(w => AdultTerms.Contains(w));
            
            // Require at least one explicit term for adult results
            return explicitTermCount >= 1;
        }

        /// <summary>
        /// Gets the adult search specificity score (how specific the adult search is)
        /// Higher score = more specific = show more adult results
        /// </summary>
        public double GetAdultSearchSpecificity(string query)
        {
            var queryLower = query.ToLower().Trim();
            var queryWords = Regex.Split(queryLower, @"\W+").Where(w => !string.IsNullOrEmpty(w)).ToList();

            double specificity = 0.0;

            // Count explicit terms
            int explicitCount = queryWords.Count(w => AdultTerms.Contains(w));
            specificity += explicitCount * 0.3;

            // Longer queries with adult terms are more specific
            if (explicitCount > 0 && queryWords.Count >= 3)
            {
                specificity += 0.2;
            }

            // Known site names are very specific
            if (AdultDomains.Any(d => queryLower.Contains(d.Replace(".com", ""))))
            {
                specificity = 1.0;
            }

            return Math.Min(specificity, 1.0);
        }
    }
}
