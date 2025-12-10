using LiteDB;
using OxideBrowserBack.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OxideBrowserBack.Services
{
    public class IndexService : IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<IndexedPage> _pages;
        private readonly ILiteCollection<TermFrequency> _termFrequencies;

        // Basic list of Spanish stop words
        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "de", "la", "que", "el", "en", "y", "a", "los", "del", "se", "las", "por", "un", "para", "con", "no", "una", "su", "al", "lo", "como", "más", "pero", "sus", "le", "ya", "o", "este", "ha", "me", "si", "sin", "sobre", "este", "entre"
        };

        public IndexService()
        {
            _db = new LiteDatabase(@"search_index.db");
            _pages = _db.GetCollection<IndexedPage>("pages");
            _termFrequencies = _db.GetCollection<TermFrequency>("term_frequencies");

            // Ensure indexes on the fields we will query
            _pages.EnsureIndex(x => x.Url);
            _termFrequencies.EnsureIndex(x => x.Term);
        }

        private readonly PorterStemmer _stemmer = new PorterStemmer();

        public void AddPageToIndex(CrawledPage page)
        {
            if (_pages.Exists(x => x.Url == page.Url))
            {
                return;
            }

            // 1. Tokenize and count (now using Stemmer)
            var fullText = $"{page.Title} {page.Content}";
            var wordCounts = TokenizeAndCount(fullText);
            
            var newPage = new IndexedPage
            {
                Url = page.Url,
                Title = page.Title,
                Content = page.Content,
                WordCount = wordCounts.Values.Sum() // Store effective length
            };

            // 2. Insert the page
            var newPageId = _pages.Insert(newPage).AsInt32;

            // 3. Store term frequencies
            var newFrequencies = new List<TermFrequency>();
            foreach (var pair in wordCounts)
            {
                newFrequencies.Add(new TermFrequency
                {
                    Term = pair.Key,
                    PageId = newPageId,
                    Frequency = pair.Value
                });
            }

            if (newFrequencies.Any())
            {
                _termFrequencies.InsertBulk(newFrequencies);
            }
        }

        private Dictionary<string, int> TokenizeAndCount(string text)
        {
            var wordCounts = new Dictionary<string, int>();
            
            var words = Regex.Split(text.ToLower(), @"\W+");

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word) || StopWords.Contains(word) || word.Length < 3)
                {
                    continue;
                }

                // Apply Stemming
                var stem = _stemmer.Stem(word);

                wordCounts.TryGetValue(stem, out int currentCount);
                wordCounts[stem] = currentCount + 1;
            }
            return wordCounts;
        }

        public IEnumerable<IndexedPage> Search(string query)
        {
            // BM25 Constants
            const double k1 = 1.2;
            const double b = 0.75;
            
            // 1. Process Query
            var queryTerms = TokenizeAndCount(query).Keys.ToList();
            if (!queryTerms.Any()) return Enumerable.Empty<IndexedPage>();

            // 2. Gather Stats
            var totalDocs = _pages.Count();
            if (totalDocs == 0) return Enumerable.Empty<IndexedPage>();
            
            // Calculate avgdl efficiently (simplistic average for now)
            // ideally cached, but query is fast enough for prototype
            var allPages = _pages.FindAll().ToList(); 
            var avgdl = allPages.Average(p => p.WordCount);
            if (avgdl == 0) avgdl = 1;

            // 3. Score Documents
            var docScores = new Dictionary<int, double>();

            foreach (var term in queryTerms)
            {
                var tfList = _termFrequencies.Find(x => x.Term == term).ToList();
                var docCountWithTerm = tfList.Count;
                
                // IDF Calculation
                var idf = Math.Log((totalDocs - docCountWithTerm + 0.5) / (docCountWithTerm + 0.5) + 1);

                foreach (var tf in tfList)
                {
                    var page = allPages.FirstOrDefault(p => p.Id == tf.PageId);
                    if (page == null) continue;

                    var freq = tf.Frequency;
                    var docLen = page.WordCount;

                    // BM25 Score component
                    var num = freq * (k1 + 1);
                    var den = freq + k1 * (1 - b + b * (docLen / avgdl));
                    var score = idf * (num / den);

                    if (!docScores.ContainsKey(page.Id)) docScores[page.Id] = 0;
                    docScores[page.Id] += score;
                }
            }

            // 4. Retrieve & Rank
            var rankedIds = docScores.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
            
            // Return actual page objects in order
            // Note: FindByIds might imply O(N) or lose order, so we explicitly map
            return rankedIds.Select(id => allPages.First(p => p.Id == id));
        }

        public bool UrlExists(string url)
        {
            return _pages.Exists(x => x.Url == url);
        }

        public IEnumerable<IndexedPage> GetByUrl(string url)
        {
            return _pages.Find(x => x.Url.StartsWith(url));
        }

        public IEnumerable<IndexedPage> GetAll()
        {
            return _pages.FindAll();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}