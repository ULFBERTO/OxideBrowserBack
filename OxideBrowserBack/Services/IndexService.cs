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

        public void AddPageToIndex(CrawledPage page)
        {
            if (_pages.Exists(x => x.Url == page.Url))
            {
                return;
            }

            var newPage = new IndexedPage
            {
                Url = page.Url,
                Title = page.Title,
                Content = page.Content
            };

            // 1. Insert the page to get its ID
            var newPageId = _pages.Insert(newPage).AsInt32;

            // 2. Tokenize and count word frequencies
            var fullText = $"{page.Title} {page.Content}";
            var wordCounts = TokenizeAndCount(fullText);

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
            
            // Split text into words using regex, removing punctuation
            var words = Regex.Split(text.ToLower(), @"\W+");

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word) || StopWords.Contains(word) || word.Length < 3)
                {
                    continue;
                }

                wordCounts.TryGetValue(word, out int currentCount);
                wordCounts[word] = currentCount + 1;
            }
            return wordCounts;
        }

        public IEnumerable<IndexedPage> Search(string query)
        {
            var searchTerm = query.ToLower();

            // 1. Find all pages containing the term and order by frequency
            var rankedPageIds = _termFrequencies.Find(x => x.Term == searchTerm)
                .OrderByDescending(x => x.Frequency)
                .Select(x => x.PageId)
                .ToList();
            
            if (!rankedPageIds.Any())
            {
                return Enumerable.Empty<IndexedPage>();
            }

            // 2. Retrieve the page details for the ranked IDs
            var pages = _pages.Find(p => rankedPageIds.Contains(p.Id));

            // 3. Order the final results according to the ranking
            // This is necessary because FindByIds doesn't preserve the order of the IDs
            return pages.OrderBy(p => rankedPageIds.IndexOf(p.Id));
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