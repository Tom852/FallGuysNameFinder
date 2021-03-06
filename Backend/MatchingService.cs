using Backend.Model;
using Common;
using Common.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend
{
    // todo: sicherstellen, dass alles immer lowercase mässig abgehandlet wird.
    // ev schon beim name oder so.
    public class MatchingService
    {
        private const string Wildcard = Pattern.Wildcard;

        public MatchingService()
        {
            this.Patterns = DataStorageStuff.ReadPatterns();
            this.Options = DataStorageStuff.GetOptions();
            this.Pool = DataStorageStuff.GetStoredPool();
        }

        public MatchingService(List<Pattern> patterns, Pool pool, Options options)
        {
            this.Patterns = patterns;
            this.Options = options;
            this.Pool = pool;
        }

        public List<Pattern> Patterns { get; }
        public Options Options { get; }
        public Pool Pool { get; }

        public MatchingResult Test(Name nameToTest)
        {
            if (Options.StopOnAlliteration && TestForAlliteration(nameToTest))
            {
                return MatchingResult.Alliteration;
            }

            if (Options.StopOnDoubleWord && TestForDoubleName(nameToTest))
            {
                return MatchingResult.DoubleWord;

            }

            if (TestForPatternMatch(nameToTest))
            {
                return MatchingResult.Pattern;
            }

            if (TestForPoolMatch(nameToTest))
            {
                return MatchingResult.Pool;
            }

            return MatchingResult.NoMatch;
        }

        private bool TestForPoolMatch(Name nameToTest)
        {
            return Pool.First.Contains(nameToTest.First) && Pool.Second.Contains(nameToTest.Second) && Pool.Third.Contains(nameToTest.Third);
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
            return pattern == Wildcard || pattern == word;
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