using Backend.Model;
using Serilog;
using System.Linq;
using System.Text.RegularExpressions;

namespace Backend
{


    public class WordProcessor
    {
        public WordProcessorResult SoftArtifactFilter(string input)
        {
            var list = input.Split(new char[] { ' ', '\n' }).Select(s => s.Trim()).Where(s => s.Length > 2).ToList();
            Log.Debug("Soft artifact filter delivered: {0}", list);
            return new WordProcessorResult(list);
        }

        public WordProcessorResult AggressiveArtifactFilter(string input)
        {
            string pattern = @"[A-Za-z]{3,}";

            Regex r = new Regex(pattern);
            var matches = r.Matches(input);

            var result = new WordProcessorResult(matches);

            Log.Debug("Aggressive artifact filter delivered {0}", result.Words);

            return result;
        }

        public WordProcessorResult WordExtractor(string input)
        {
            //string pattern = @"([A-Z][a-z]{2,}|VIP|MVP|IceCream)";  // das macht nun eig case invariatn
            string pattern = @"([A-Z][a-z]{2,})";

            Regex r = new Regex(pattern);
            var matches = r.Matches(input);

            var result = new WordProcessorResult(matches);
            Log.Debug("Word-Extractor delivered {0}", result.Words);

            return result;
        }
    }
}