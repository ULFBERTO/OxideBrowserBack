namespace OxideBrowserBack.Services
{
    /// <summary>
    /// Enhanced Porter Stemming Algorithm with Spanish support.
    /// Reduces words to their root form (e.g. "running" -> "run", "corriendo" -> "corr").
    /// </summary>
    public class PorterStemmer
    {
        // Spanish suffixes for stemming
        private static readonly string[] SpanishSuffixes = {
            "amiento", "imientos", "amiento", "aciones", "uciones", "adores", "adoras",
            "ancias", "encias", "mente", "ables", "ibles", "istas", "adora", "ación",
            "ución", "ancia", "encia", "ador", "ante", "able", "ible", "ista", "oso",
            "osa", "ivo", "iva", "ando", "iendo", "ado", "ido", "ar", "er", "ir"
        };

        public string Stem(string word)
        {
            if (string.IsNullOrWhiteSpace(word) || word.Length <= 2) return word;
            
            word = word.ToLowerInvariant();

            // Detect language and apply appropriate stemmer
            if (IsLikelySpanish(word))
            {
                return StemSpanish(word);
            }

            return StemEnglish(word);
        }

        private bool IsLikelySpanish(string word)
        {
            // Check for Spanish-specific patterns
            return word.Contains("ñ") || 
                   word.EndsWith("ción") || 
                   word.EndsWith("ando") || 
                   word.EndsWith("iendo") ||
                   word.EndsWith("mente");
        }

        private string StemSpanish(string word)
        {
            if (word.Length <= 3) return word;

            // Remove common Spanish suffixes
            foreach (var suffix in SpanishSuffixes)
            {
                if (word.EndsWith(suffix) && word.Length - suffix.Length >= 2)
                {
                    return word.Substring(0, word.Length - suffix.Length);
                }
            }

            return word;
        }

        private string StemEnglish(string word)
        {
            // Step 1a
            if (word.EndsWith("sses")) word = word.Substring(0, word.Length - 2);
            else if (word.EndsWith("ies")) word = word.Substring(0, word.Length - 2);
            else if (word.EndsWith("ss")) { /* do nothing */ }
            else if (word.EndsWith("s") && word.Length > 3) word = word.Substring(0, word.Length - 1);

            // Step 1b
            if (word.EndsWith("eed"))
            {
                if (Measure(word.Substring(0, word.Length - 3)) > 0)
                    word = word.Substring(0, word.Length - 1);
            }
            else if ((word.EndsWith("ed") && ContainsVowel(word.Substring(0, word.Length - 2))) ||
                     (word.EndsWith("ing") && ContainsVowel(word.Substring(0, word.Length - 3))))
            {
                if (word.EndsWith("ed")) word = word.Substring(0, word.Length - 2);
                else word = word.Substring(0, word.Length - 3);

                if (word.EndsWith("at") || word.EndsWith("bl") || word.EndsWith("iz"))
                {
                    word += "e";
                }
                else if (EndsWithDoubleConsonant(word) && !word.EndsWith("l") && !word.EndsWith("s") && !word.EndsWith("z"))
                {
                    word = word.Substring(0, word.Length - 1);
                }
                else if (Measure(word) == 1 && EndsWithCVC(word))
                {
                    word += "e";
                }
            }

            // Step 1c
            if (word.EndsWith("y") && ContainsVowel(word.Substring(0, word.Length - 1)))
            {
                word = word.Substring(0, word.Length - 1) + "i";
            }

            // Step 2
            word = Step2(word);

            // Step 3
            word = Step3(word);

            // Step 4
            word = Step4(word);

            // Step 5a
            if (word.EndsWith("e"))
            {
                var stem = word.Substring(0, word.Length - 1);
                if (Measure(stem) > 1 || (Measure(stem) == 1 && !EndsWithCVC(stem)))
                {
                    word = stem;
                }
            }

            // Step 5b
            if (word.EndsWith("ll") && Measure(word) > 1)
            {
                word = word.Substring(0, word.Length - 1);
            }

            return word;
        }

        private string Step2(string word)
        {
            var suffixes = new Dictionary<string, string>
            {
                {"ational", "ate"}, {"tional", "tion"}, {"enci", "ence"}, {"anci", "ance"},
                {"izer", "ize"}, {"abli", "able"}, {"alli", "al"}, {"entli", "ent"},
                {"eli", "e"}, {"ousli", "ous"}, {"ization", "ize"}, {"ation", "ate"},
                {"ator", "ate"}, {"alism", "al"}, {"iveness", "ive"}, {"fulness", "ful"},
                {"ousness", "ous"}, {"aliti", "al"}, {"iviti", "ive"}, {"biliti", "ble"}
            };

            foreach (var pair in suffixes)
            {
                if (word.EndsWith(pair.Key))
                {
                    var stem = word.Substring(0, word.Length - pair.Key.Length);
                    if (Measure(stem) > 0)
                    {
                        return stem + pair.Value;
                    }
                }
            }
            return word;
        }

        private string Step3(string word)
        {
            var suffixes = new Dictionary<string, string>
            {
                {"icate", "ic"}, {"ative", ""}, {"alize", "al"}, {"iciti", "ic"},
                {"ical", "ic"}, {"ful", ""}, {"ness", ""}
            };

            foreach (var pair in suffixes)
            {
                if (word.EndsWith(pair.Key))
                {
                    var stem = word.Substring(0, word.Length - pair.Key.Length);
                    if (Measure(stem) > 0)
                    {
                        return stem + pair.Value;
                    }
                }
            }
            return word;
        }

        private string Step4(string word)
        {
            var suffixes = new[] { "al", "ance", "ence", "er", "ic", "able", "ible", "ant", 
                "ement", "ment", "ent", "ion", "ou", "ism", "ate", "iti", "ous", "ive", "ize" };

            foreach (var suffix in suffixes)
            {
                if (word.EndsWith(suffix))
                {
                    var stem = word.Substring(0, word.Length - suffix.Length);
                    if (Measure(stem) > 1)
                    {
                        if (suffix == "ion" && stem.Length > 0 && (stem.EndsWith("s") || stem.EndsWith("t")))
                        {
                            return stem;
                        }
                        else if (suffix != "ion")
                        {
                            return stem;
                        }
                    }
                }
            }
            return word;
        }

        private int Measure(string stem)
        {
            int m = 0;
            bool v = false;
            for (int i = 0; i < stem.Length; i++)
            {
                if (IsVowel(stem[i])) v = true;
                else if (v) { m++; v = false; }
            }
            return m;
        }

        private bool ContainsVowel(string stem) => stem.Any(IsVowel);

        private bool IsVowel(char c) => "aeiou".Contains(c);

        private bool EndsWithDoubleConsonant(string stem)
        {
            if (stem.Length < 2) return false;
            return stem[stem.Length - 1] == stem[stem.Length - 2] && !IsVowel(stem[stem.Length - 1]);
        }

        private bool EndsWithCVC(string stem)
        {
            if (stem.Length < 3) return false;
            char last = stem[stem.Length - 1];
            char secondLast = stem[stem.Length - 2];
            char thirdLast = stem[stem.Length - 3];
            
            return !IsVowel(last) && IsVowel(secondLast) && !IsVowel(thirdLast) && !"wxy".Contains(last);
        }
    }
}
