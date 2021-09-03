using Common.Model;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace Backend
{
    // todo: sicherstellen, dass alles immer lowercase mässig abgehandlet wird.
    // ev schon beim name oder so.
    public class MatchingService
    {
        private const string Wildcard = "*";

        public MatchingService()
        {
            this.Patterns = DataStorageStuff.ReadPatterns();
            this.Options = DataStorageStuff.GetOptions();
        }

        public MatchingService(List<Pattern> patterns)
        {
            Patterns = patterns;
        }

        public List<Pattern> Patterns { get; }
        public Options Options { get; }

        public bool Test(Name nameToTest)
        {
            if (Options.StopOnAlliteration && TestForAlliteration(nameToTest))
            {
                Log.Information("Alliteration recognized");
                return true;
            }

            if (Options.StopOnDoubleWord && TestForDoubleName(nameToTest))
            {
                Log.Information("Double-word recognized");
                return true;
            }

            var patternMatch = TestForPatternMatch(nameToTest);
            if (patternMatch)
            {
                Log.Information("Pattern match recognized");
                return true;
            }

            return false;
        }

        private bool TestForPatternMatch(Name toTest)
        {
            return Patterns.Any(pattern => Matches(pattern, toTest));
        }

        private bool Matches(Pattern pattern, Name toTest)
        {
            return Matches(pattern.First, toTest.First) && Matches(pattern.Second, toTest.Second) && Matches(pattern.Third, toTest.Third);
        }

        private bool Matches(string pattern, string word)
        {
            return pattern == Wildcard || pattern.ToLower().Trim() == word.ToLower().Trim();
        }

        private bool TestForAlliteration(Name toTest)
        {
            var character = toTest.First.ToLower()[0];
            return toTest.Second.ToLower()[0] == character && toTest.Third.ToLower()[0] == character;
        }

        private bool TestForDoubleName(Name toTest)
        {
            var option1 = new string(toTest.First.ToLower().Take(4).ToArray());
            var option2 = new string(toTest.Second.ToLower().Take(4).ToArray());

            var option1Fits = toTest.Second.ToLower().StartsWith(option1) || toTest.Third.ToLower().StartsWith(option1);
            var option2Fits = toTest.Third.ToLower().StartsWith(option2);
            return option1Fits || option2Fits;
        }
    }
}