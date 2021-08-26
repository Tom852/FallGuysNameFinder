using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Tesseract;
using Serilog;
using System.Text.RegularExpressions;
using Common.Model;

namespace Backend
{
    public class WordsComparisonService
    {
        const string Wildcard = "*";

        public WordsComparisonService(DataStorageStuff storageAccess)
        {
            this.Patterns = storageAccess.ReadPatterns();
            this.Options = storageAccess.GetOptions();
        }

        public WordsComparisonService(List<Pattern> patterns)
        {
            Patterns = patterns;
        }

        public List<Pattern> Patterns { get; }
        public Options Options { get; }


        public bool Test(string[] s)
        {

            var nameToTest = new ParsedName(s);
            if (Options.StopOnAlliteration && TestForAlliteration(nameToTest))
            {
                Log.Information("Alliteration detected");
                return true;
            }

            if (Options.StopOnDoubleWord && TestForDoubleName(nameToTest))
            {
                Log.Information("Double-word detected");
                return true;
            }

            var patternMatch = TestForPatternMatch(nameToTest);
            if (patternMatch)
            {
                Log.Information("Pattern matched");
            }
            else
            {
                Log.Information("No Pattern matched");
            }
            return patternMatch;
        }

        private bool TestForPatternMatch(ParsedName nameToTest)
        {
            return Patterns.Any(pattern => Matches(pattern, nameToTest));
        }

        private bool Matches(Pattern pattern, ParsedName toTest)
        {
            return Matches(pattern.First, toTest.First) && Matches(pattern.Second, toTest.Second) && Matches(pattern.Third, toTest.Third);
        }


        private bool Matches(string pattern, string name)
        {
            return pattern == Wildcard || pattern.ToLower().Trim() == name.ToLower().Trim();
        }

        private bool TestForAlliteration(ParsedName toTest)
        {
            var character = toTest.First[0];
            return toTest.Second[0] == character && toTest.Third[0] == character;
        }

        private bool TestForDoubleName(ParsedName toTest)
        {
            var option1 = new string(toTest.First.Take(4).ToArray());
            var option2 = new string(toTest.Second.Take(4).ToArray());

            var option1Fits = toTest.Second.StartsWith(option1) || toTest.Third.StartsWith(option1);
            var option2Fits = toTest.Third.StartsWith(option2);
            return option1Fits || option2Fits;
        }

    }
}
