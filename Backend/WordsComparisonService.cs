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

namespace Backend
{
    public class WordsComparisonService
    {

        public WordsComparisonService(DataStorageStuff storageAccess)
        {
            this.Patterns = storageAccess.Read();
        }

        public WordsComparisonService(List<Pattern> patterns)
        {
            Patterns = patterns;
        }

        public List<Pattern> Patterns { get; }

        public bool Test(string[] s)
        {

            var nameToTest = new Name(s);

            return this.Patterns.Any(goodName => Matches(goodName, nameToTest));


        }

        private bool Matches(Pattern pattern, Name toTest)
        {

            if (TestForAlliteration(toTest))
            {
                Log.Information("Alliteration Found");
                return true;
            }
            return Matches(pattern.First, toTest.First) && Matches(pattern.Second, toTest.Second) && Matches(pattern.Third, toTest.Third);
        }

        private bool TestForAlliteration(Name toTest)
        {
            var character = toTest.First[0];
            return toTest.Second[0] == character && toTest.Third[0] == character;
        }

        private bool Matches(string pattern, string name)
        {
            Match match = Regex.Match(name, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                Log.Debug("{0} did match to {1}", name, pattern);
                return true;
            }
            else
            {
                Log.Debug("{0} did not match to {1}", name, pattern);
                return false;
            }
        }

    }
}
