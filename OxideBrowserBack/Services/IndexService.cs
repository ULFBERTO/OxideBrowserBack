using LiteDB;
using OxideBrowserBack.Models;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace OxideBrowserBack.Services
{
    public class IndexService : IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<IndexedPage> _pages;
        private readonly ILiteCollection<TermFrequency> _termFrequencies;
        private readonly ILiteCollection<NGramFrequency> _ngramFrequencies;
        private readonly PorterStemmer _stemmer = new();
        private readonly ContentFilterService _contentFilter = new();

        // Extended Spanish + English stop words
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Spanish
            "de", "la", "que", "el", "en", "y", "a", "los", "del", "se", "las", "por", 
            "un", "para", "con", "no", "una", "su", "al", "lo", "como", "más", "pero", 
            "sus", "le", "ya", "o", "este", "ha", "me", "si", "sin", "sobre", "entre",
            "cuando", "muy", "ser", "es", "también", "fue", "había", "todo", "esta",
            // English
            "the", "be", "to", "of", "and", "a", "in", "that", "have", "i", "it", "for",
            "not", "on", "with", "he", "as", "you", "do", "at", "this", "but", "his",
            "by", "from", "they", "we", "say", "her", "she", "or", "an", "will", "my",
            "one", "all", "would", "there", "their", "what", "so", "up", "out", "if",
            "about", "who", "get", "which", "go", "me", "when", "can", "like", "just"
        };

        // Synonyms dictionary for query expansion
        private static readonly Dictionary<string, List<string>> Synonyms = new(StringComparer.OrdinalIgnoreCase)
        {
            { "comprar", new() { "adquirir", "obtener", "conseguir", "purchase", "buy" } },
            { "buy", new() { "purchase", "acquire", "get", "comprar" } },
            { "buscar", new() { "encontrar", "search", "find", "look" } },
            { "search", new() { "find", "look", "buscar", "seek" } },
            { "gratis", new() { "free", "gratuito", "sin costo" } },
            { "free", new() { "gratis", "gratuito", "libre" } },
            { "mejor", new() { "best", "top", "superior", "óptimo" } },
            { "best", new() { "mejor", "top", "greatest", "finest" } },
            { "barato", new() { "cheap", "económico", "asequible" } },
            { "cheap", new() { "barato", "affordable", "inexpensive" } },
            { "rápido", new() { "fast", "quick", "veloz" } },
            { "fast", new() { "rápido", "quick", "swift", "speedy" } }
        };

        public IndexService()
        {
            _db = new LiteDatabase(@"search_index.db");
            _pages = _db.GetCollection<IndexedPage>("pages");
            _termFrequencies = _db.GetCollection<TermFrequency>("term_frequencies");
            _ngramFrequencies = _db.GetCollection<NGramFrequency>("ngram_frequencies");

            // Ensure indexes
            _pages.EnsureIndex(x => x.Url);
            _pages.EnsureIndex(x => x.Domain);
            _pages.EnsureIndex(x => x.IsAdultContent);
            _termFrequencies.EnsureIndex(x => x.Term);
            _termFrequencies.EnsureIndex(x => x.PageId);
            _ngramFrequencies.EnsureIndex(x => x.NGram);
            _ngramFrequencies.EnsureIndex(x => x.PageId);
        }

        public void AddPageToIndex(CrawledPage page)
        {
            var existingPage = _pages.FindOne(x => x.Url == page.Url);
            if (existingPage != null)
            {
                // Update existing page
                UpdatePageIndex(existingPage.Id, page);
                return;
            }

            // Analyze content for adult classification
            var (isAdult, adultScore) = _contentFilter.AnalyzeContent(page.Url, page.Title, page.Content);

            // Extract domain
            string? domain = null;
            try
            {
                domain = new Uri(page.Url).Host.ToLower();
            }
            catch { }

            // Tokenize content
            var fullText = $"{page.Title} {page.Content}";
            var wordCounts = TokenizeAndCount(fullText);
            var titleTerms = TokenizeAndCount(page.Title).Keys.ToHashSet();
            var urlTerms = TokenizeAndCount(page.Url.Replace("/", " ").Replace("-", " ")).Keys.ToHashSet();

            // Calculate quality score
            double qualityScore = CalculateContentQuality(page.Title, page.Content, page.MetaDescription);

            var newPage = new IndexedPage
            {
                Url = page.Url,
                Title = page.Title,
                Content = page.Content,
                WordCount = wordCounts.Values.Sum(),
                IsAdultContent = isAdult,
                AdultContentScore = adultScore,
                Domain = domain,
                IndexedAt = DateTime.UtcNow,
                OutboundLinks = page.Links.Count,
                TitleLength = page.Title.Length,
                HasMetaDescription = !string.IsNullOrEmpty(page.MetaDescription),
                MetaDescription = page.MetaDescription,
                MetaKeywords = page.MetaKeywords,
                ContentQualityScore = qualityScore
            };

            var newPageId = _pages.Insert(newPage).AsInt32;

            // Store term frequencies with position info
            StoreTermFrequencies(newPageId, wordCounts, titleTerms, urlTerms, fullText);

            // Store n-grams for phrase matching
            StoreNGrams(newPageId, fullText);

            // Update inbound links for linked pages
            UpdateInboundLinks(page.Links);
        }


        private void UpdatePageIndex(int pageId, CrawledPage page)
        {
            var existingPage = _pages.FindById(pageId);
            if (existingPage == null) return;

            // Remove old term frequencies
            _termFrequencies.DeleteMany(x => x.PageId == pageId);
            _ngramFrequencies.DeleteMany(x => x.PageId == pageId);

            // Re-analyze content
            var (isAdult, adultScore) = _contentFilter.AnalyzeContent(page.Url, page.Title, page.Content);
            var fullText = $"{page.Title} {page.Content}";
            var wordCounts = TokenizeAndCount(fullText);
            var titleTerms = TokenizeAndCount(page.Title).Keys.ToHashSet();
            var urlTerms = TokenizeAndCount(page.Url.Replace("/", " ").Replace("-", " ")).Keys.ToHashSet();

            // Update page
            existingPage.Title = page.Title;
            existingPage.Content = page.Content;
            existingPage.WordCount = wordCounts.Values.Sum();
            existingPage.IsAdultContent = isAdult;
            existingPage.AdultContentScore = adultScore;
            existingPage.LastUpdated = DateTime.UtcNow;
            existingPage.OutboundLinks = page.Links.Count;
            existingPage.TitleLength = page.Title.Length;
            existingPage.HasMetaDescription = !string.IsNullOrEmpty(page.MetaDescription);
            existingPage.MetaDescription = page.MetaDescription;
            existingPage.MetaKeywords = page.MetaKeywords;
            existingPage.ContentQualityScore = CalculateContentQuality(page.Title, page.Content, page.MetaDescription);

            _pages.Update(existingPage);

            // Re-store frequencies
            StoreTermFrequencies(pageId, wordCounts, titleTerms, urlTerms, fullText);
            StoreNGrams(pageId, fullText);
        }

        private void StoreTermFrequencies(int pageId, Dictionary<string, int> wordCounts, 
            HashSet<string> titleTerms, HashSet<string> urlTerms, string fullText)
        {
            var frequencies = new List<TermFrequency>();
            var textLower = fullText.ToLower();

            foreach (var pair in wordCounts)
            {
                var tf = new TermFrequency
                {
                    Term = pair.Key,
                    PageId = pageId,
                    Frequency = pair.Value,
                    InTitle = titleTerms.Contains(pair.Key),
                    InUrl = urlTerms.Contains(pair.Key),
                    FirstPosition = textLower.IndexOf(pair.Key, StringComparison.OrdinalIgnoreCase)
                };
                frequencies.Add(tf);
            }

            if (frequencies.Any())
            {
                _termFrequencies.InsertBulk(frequencies);
            }
        }

        private void StoreNGrams(int pageId, string text)
        {
            var words = Regex.Split(text.ToLower(), @"\W+")
                .Where(w => !string.IsNullOrWhiteSpace(w) && w.Length >= 2)
                .ToList();

            var ngrams = new Dictionary<string, int>();

            // Bigrams
            for (int i = 0; i < words.Count - 1; i++)
            {
                if (StopWords.Contains(words[i]) && StopWords.Contains(words[i + 1])) continue;
                var bigram = $"{words[i]} {words[i + 1]}";
                ngrams.TryGetValue(bigram, out int count);
                ngrams[bigram] = count + 1;
            }

            // Trigrams
            for (int i = 0; i < words.Count - 2; i++)
            {
                if (StopWords.Contains(words[i]) && StopWords.Contains(words[i + 2])) continue;
                var trigram = $"{words[i]} {words[i + 1]} {words[i + 2]}";
                ngrams.TryGetValue(trigram, out int count);
                ngrams[trigram] = count + 1;
            }

            var ngramFreqs = ngrams.Select(ng => new NGramFrequency
            {
                NGram = ng.Key,
                PageId = pageId,
                Frequency = ng.Value,
                NGramSize = ng.Key.Split(' ').Length
            }).ToList();

            if (ngramFreqs.Any())
            {
                _ngramFrequencies.InsertBulk(ngramFreqs);
            }
        }

        private void UpdateInboundLinks(List<string> links)
        {
            foreach (var link in links.Distinct().Take(100)) // Limit to prevent spam
            {
                try
                {
                    var uri = new Uri(link);
                    var page = _pages.FindOne(x => x.Url == uri.AbsoluteUri);
                    if (page != null)
                    {
                        page.InboundLinks++;
                        _pages.Update(page);
                    }
                }
                catch { }
            }
        }

        private double CalculateContentQuality(string title, string content, string? metaDescription)
        {
            double score = 0.5; // Base score

            // Title quality
            if (title.Length >= 10 && title.Length <= 70) score += 0.1;
            if (!string.IsNullOrEmpty(title) && !title.Contains("Untitled")) score += 0.05;

            // Content length
            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount >= 100) score += 0.1;
            if (wordCount >= 500) score += 0.1;
            if (wordCount >= 1000) score += 0.05;

            // Meta description
            if (!string.IsNullOrEmpty(metaDescription)) score += 0.1;

            return Math.Min(score, 1.0);
        }

        private Dictionary<string, int> TokenizeAndCount(string text)
        {
            var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var words = Regex.Split(text.ToLower(), @"\W+");

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word) || StopWords.Contains(word) || word.Length < 2)
                    continue;

                var stem = _stemmer.Stem(word);
                wordCounts.TryGetValue(stem, out int currentCount);
                wordCounts[stem] = currentCount + 1;
            }
            return wordCounts;
        }


        public IEnumerable<IndexedPage> Search(string query, bool allowAdultContent = false)
        {
            // BM25 Constants
            const double k1 = 1.5;
            const double b = 0.75;

            // Boost factors
            const double titleBoost = 2.5;
            const double urlBoost = 1.8;
            const double phraseBoost = 3.0;
            const double freshnessBoost = 1.2;
            const double qualityBoost = 1.5;
            const double inboundLinkBoost = 0.1;

            // Check if adult content should be shown
            bool isExplicitAdultSearch = _contentFilter.IsExplicitAdultSearch(query);
            double adultSpecificity = _contentFilter.GetAdultSearchSpecificity(query);

            // Process query with expansion
            var queryTerms = TokenizeAndCount(query).Keys.ToList();
            var expandedTerms = ExpandQueryWithSynonyms(queryTerms);
            
            if (!queryTerms.Any()) return Enumerable.Empty<IndexedPage>();

            // Get all pages (filtered by adult content rules)
            var allPages = _pages.FindAll().ToList();
            
            // Filter adult content unless explicitly searched
            if (!allowAdultContent && !isExplicitAdultSearch)
            {
                allPages = allPages.Where(p => !p.IsAdultContent).ToList();
            }
            else if (isExplicitAdultSearch)
            {
                // For explicit adult searches, include adult content but rank by specificity
                // Low specificity = adult content ranked lower
                // High specificity = adult content ranked normally
            }

            var totalDocs = allPages.Count;
            if (totalDocs == 0) return Enumerable.Empty<IndexedPage>();

            var avgdl = allPages.Average(p => Math.Max(p.WordCount, 1));
            var docScores = new ConcurrentDictionary<int, double>();

            // Score by individual terms (BM25)
            foreach (var term in expandedTerms)
            {
                var tfList = _termFrequencies.Find(x => x.Term == term).ToList();
                var docCountWithTerm = tfList.Count;
                if (docCountWithTerm == 0) continue;

                var idf = Math.Log((totalDocs - docCountWithTerm + 0.5) / (docCountWithTerm + 0.5) + 1);

                foreach (var tf in tfList)
                {
                    var page = allPages.FirstOrDefault(p => p.Id == tf.PageId);
                    if (page == null) continue;

                    var freq = tf.Frequency;
                    var docLen = Math.Max(page.WordCount, 1);

                    // BM25 base score
                    var num = freq * (k1 + 1);
                    var den = freq + k1 * (1 - b + b * (docLen / avgdl));
                    var score = idf * (num / den);

                    // Apply boosts
                    if (tf.InTitle) score *= titleBoost;
                    if (tf.InUrl) score *= urlBoost;

                    // Position boost (earlier = better)
                    if (tf.FirstPosition >= 0 && tf.FirstPosition < 500)
                    {
                        score *= 1.0 + (0.3 * (1.0 - tf.FirstPosition / 500.0));
                    }

                    docScores.AddOrUpdate(page.Id, score, (_, existing) => existing + score);
                }
            }

            // Phrase matching boost (n-grams) - LiteDB compatible queries
            var queryPhrase = string.Join(" ", queryTerms);
            var matchingNgrams = new List<NGramFrequency>();
            
            // Search for exact phrase match
            if (!string.IsNullOrEmpty(queryPhrase))
            {
                matchingNgrams.AddRange(_ngramFrequencies.Find(x => x.NGram.Contains(queryPhrase)).ToList());
            }
            
            // Search for individual terms in n-grams (separate queries for LiteDB compatibility)
            foreach (var term in queryTerms.Take(5)) // Limit to avoid too many queries
            {
                var termNgrams = _ngramFrequencies.Find(x => x.NGram.Contains(term)).ToList();
                matchingNgrams.AddRange(termNgrams);
            }
            
            // Deduplicate
            matchingNgrams = matchingNgrams.DistinctBy(n => n.Id).ToList();

            foreach (var ngram in matchingNgrams)
            {
                if (docScores.ContainsKey(ngram.PageId))
                {
                    var phraseScore = ngram.Frequency * phraseBoost * (ngram.NGramSize / 2.0);
                    docScores.AddOrUpdate(ngram.PageId, phraseScore, (_, existing) => existing + phraseScore);
                }
            }

            // Track how many query terms each document matches
            var termMatchCount = new Dictionary<int, int>();
            foreach (var term in queryTerms) // Use original terms, not expanded
            {
                var tfList = _termFrequencies.Find(x => x.Term == term).ToList();
                foreach (var tf in tfList)
                {
                    if (!termMatchCount.ContainsKey(tf.PageId))
                        termMatchCount[tf.PageId] = 0;
                    termMatchCount[tf.PageId]++;
                }
            }

            // Apply page-level ranking factors
            foreach (var pageId in docScores.Keys.ToList())
            {
                var page = allPages.FirstOrDefault(p => p.Id == pageId);
                if (page == null) continue;

                var currentScore = docScores[pageId];

                // CRITICAL: Boost for matching ALL query terms (multi-word queries)
                if (queryTerms.Count > 1)
                {
                    var matchedTerms = termMatchCount.GetValueOrDefault(pageId, 0);
                    var matchRatio = (double)matchedTerms / queryTerms.Count;
                    
                    if (matchRatio >= 1.0)
                    {
                        // All terms match - big boost
                        currentScore *= 5.0;
                    }
                    else if (matchRatio >= 0.5)
                    {
                        // Most terms match - moderate boost
                        currentScore *= (1.0 + matchRatio * 2.0);
                    }
                    else
                    {
                        // Few terms match - penalty (will appear at end)
                        currentScore *= matchRatio * 0.5;
                    }
                }

                // Quality boost
                currentScore *= (1.0 + (page.ContentQualityScore - 0.5) * qualityBoost);

                // Inbound links boost (logarithmic to prevent spam)
                if (page.InboundLinks > 0)
                {
                    currentScore *= (1.0 + Math.Log10(page.InboundLinks + 1) * inboundLinkBoost);
                }

                // Freshness boost (newer content slightly preferred)
                var daysSinceIndex = (DateTime.UtcNow - page.IndexedAt).TotalDays;
                if (daysSinceIndex < 30)
                {
                    currentScore *= freshnessBoost;
                }
                else if (daysSinceIndex < 90)
                {
                    currentScore *= 1.0 + (freshnessBoost - 1.0) * 0.5;
                }

                // Domain trust boost
                currentScore *= page.DomainTrustScore;

                // PageRank factor
                currentScore *= page.PageRank;

                // Adult content penalty for non-explicit searches
                if (page.IsAdultContent && isExplicitAdultSearch && adultSpecificity < 0.5)
                {
                    currentScore *= adultSpecificity;
                }

                docScores[pageId] = currentScore;
            }

            // Rank and return
            var rankedIds = docScores.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
            return rankedIds.Select(id => allPages.First(p => p.Id == id));
        }

        private List<string> ExpandQueryWithSynonyms(List<string> queryTerms)
        {
            var expanded = new HashSet<string>(queryTerms, StringComparer.OrdinalIgnoreCase);

            foreach (var term in queryTerms)
            {
                if (Synonyms.TryGetValue(term, out var synonyms))
                {
                    foreach (var syn in synonyms.Take(3)) // Limit expansion
                    {
                        var stemmed = _stemmer.Stem(syn);
                        expanded.Add(stemmed);
                    }
                }
            }

            return expanded.ToList();
        }


        /// <summary>
        /// Reindex all existing pages - recalculates rankings, adult content scores, and term frequencies
        /// </summary>
        public async Task<ReindexResult> ReindexAllPagesAsync(IProgress<ReindexProgress>? progress = null)
        {
            var result = new ReindexResult();
            var allPages = _pages.FindAll().ToList();
            result.TotalPages = allPages.Count;

            int processed = 0;
            foreach (var page in allPages)
            {
                try
                {
                    // Re-analyze adult content
                    var (isAdult, adultScore) = _contentFilter.AnalyzeContent(page.Url, page.Title, page.Content);
                    
                    // Recalculate quality
                    var qualityScore = CalculateContentQuality(page.Title, page.Content, page.MetaDescription);

                    // Delete old frequencies
                    _termFrequencies.DeleteMany(x => x.PageId == page.Id);
                    _ngramFrequencies.DeleteMany(x => x.PageId == page.Id);

                    // Recalculate term frequencies
                    var fullText = $"{page.Title} {page.Content}";
                    var wordCounts = TokenizeAndCount(fullText);
                    var titleTerms = TokenizeAndCount(page.Title).Keys.ToHashSet();
                    var urlTerms = TokenizeAndCount(page.Url.Replace("/", " ").Replace("-", " ")).Keys.ToHashSet();

                    // Update page
                    page.IsAdultContent = isAdult;
                    page.AdultContentScore = adultScore;
                    page.ContentQualityScore = qualityScore;
                    page.WordCount = wordCounts.Values.Sum();
                    page.LastUpdated = DateTime.UtcNow;

                    _pages.Update(page);

                    // Re-store frequencies
                    StoreTermFrequencies(page.Id, wordCounts, titleTerms, urlTerms, fullText);
                    StoreNGrams(page.Id, fullText);

                    result.SuccessCount++;
                    processed++;

                    progress?.Report(new ReindexProgress
                    {
                        Processed = processed,
                        Total = result.TotalPages,
                        CurrentUrl = page.Url
                    });
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"{page.Url}: {ex.Message}");
                }
            }

            // Recalculate PageRank
            await CalculatePageRankAsync();

            return result;
        }

        /// <summary>
        /// Reindex a specific page by URL
        /// </summary>
        public bool ReindexPage(string url)
        {
            var page = _pages.FindOne(x => x.Url == url);
            if (page == null) return false;

            try
            {
                var (isAdult, adultScore) = _contentFilter.AnalyzeContent(page.Url, page.Title, page.Content);
                var qualityScore = CalculateContentQuality(page.Title, page.Content, page.MetaDescription);

                _termFrequencies.DeleteMany(x => x.PageId == page.Id);
                _ngramFrequencies.DeleteMany(x => x.PageId == page.Id);

                var fullText = $"{page.Title} {page.Content}";
                var wordCounts = TokenizeAndCount(fullText);
                var titleTerms = TokenizeAndCount(page.Title).Keys.ToHashSet();
                var urlTerms = TokenizeAndCount(page.Url.Replace("/", " ").Replace("-", " ")).Keys.ToHashSet();

                page.IsAdultContent = isAdult;
                page.AdultContentScore = adultScore;
                page.ContentQualityScore = qualityScore;
                page.WordCount = wordCounts.Values.Sum();
                page.LastUpdated = DateTime.UtcNow;

                _pages.Update(page);
                StoreTermFrequencies(page.Id, wordCounts, titleTerms, urlTerms, fullText);
                StoreNGrams(page.Id, fullText);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Simplified PageRank calculation
        /// </summary>
        private Task CalculatePageRankAsync()
        {
            return Task.Run(() =>
            {
                const double dampingFactor = 0.85;
                const int iterations = 10;

                var allPages = _pages.FindAll().ToList();
                var pageRanks = allPages.ToDictionary(p => p.Id, _ => 1.0);
                var totalPages = allPages.Count;

                if (totalPages == 0) return;

                for (int i = 0; i < iterations; i++)
                {
                    var newRanks = new Dictionary<int, double>();

                    foreach (var page in allPages)
                    {
                        double rank = (1 - dampingFactor) / totalPages;

                        // Sum contributions from pages linking to this one
                        var linkingPages = allPages.Where(p => p.OutboundLinks > 0).ToList();
                        foreach (var linkingPage in linkingPages)
                        {
                            if (linkingPage.OutboundLinks > 0)
                            {
                                rank += dampingFactor * (pageRanks[linkingPage.Id] / linkingPage.OutboundLinks);
                            }
                        }

                        newRanks[page.Id] = rank;
                    }

                    pageRanks = newRanks;
                }

                // Normalize and update
                var maxRank = pageRanks.Values.Max();
                foreach (var page in allPages)
                {
                    page.PageRank = maxRank > 0 ? pageRanks[page.Id] / maxRank : 1.0;
                    _pages.Update(page);
                }
            });
        }

        public bool UrlExists(string url) => _pages.Exists(x => x.Url == url);

        public IEnumerable<IndexedPage> GetByUrl(string url) => _pages.Find(x => x.Url.StartsWith(url));

        public IEnumerable<IndexedPage> GetAll() => _pages.FindAll();

        public IndexStats GetStats()
        {
            var pages = _pages.FindAll().ToList();
            return new IndexStats
            {
                TotalPages = pages.Count,
                TotalTerms = _termFrequencies.Count(),
                TotalNGrams = _ngramFrequencies.Count(),
                AdultPages = pages.Count(p => p.IsAdultContent),
                AverageQualityScore = pages.Any() ? pages.Average(p => p.ContentQualityScore) : 0,
                LastIndexed = pages.Any() ? pages.Max(p => p.IndexedAt) : null,
                UniqueDomains = pages.Select(p => p.Domain).Distinct().Count()
            };
        }

        /// <summary>
        /// Clear all indexed pages
        /// </summary>
        public ClearIndexResult ClearAll()
        {
            var count = _pages.Count();
            _pages.DeleteAll();
            _termFrequencies.DeleteAll();
            _ngramFrequencies.DeleteAll();
            
            return new ClearIndexResult
            {
                PagesDeleted = count,
                Message = $"Cleared {count} pages from index"
            };
        }

        /// <summary>
        /// Clear pages by domain
        /// </summary>
        public ClearIndexResult ClearByDomain(string domain)
        {
            var pagesToDelete = _pages.Find(x => x.Domain != null && x.Domain.Contains(domain)).ToList();
            var count = 0;

            foreach (var page in pagesToDelete)
            {
                _termFrequencies.DeleteMany(x => x.PageId == page.Id);
                _ngramFrequencies.DeleteMany(x => x.PageId == page.Id);
                _pages.Delete(page.Id);
                count++;
            }

            return new ClearIndexResult
            {
                PagesDeleted = count,
                Message = $"Cleared {count} pages from domain '{domain}'"
            };
        }

        public void Dispose() => _db.Dispose();
    }

    public class ClearIndexResult
    {
        public int PagesDeleted { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ReindexResult
    {
        public int TotalPages { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ReindexProgress
    {
        public int Processed { get; set; }
        public int Total { get; set; }
        public string? CurrentUrl { get; set; }
    }

    public class IndexStats
    {
        public int TotalPages { get; set; }
        public int TotalTerms { get; set; }
        public int TotalNGrams { get; set; }
        public int AdultPages { get; set; }
        public double AverageQualityScore { get; set; }
        public DateTime? LastIndexed { get; set; }
        public int UniqueDomains { get; set; }
    }
}
