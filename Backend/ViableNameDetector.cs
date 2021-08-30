using Common.Model;
using System;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class ViableNameDetector
    {
        public Name LastMatch { get; set; }

        public bool TestForViableName(params WordProcessorResult[] inputs)
        {
            foreach (var input in inputs)
            {
                var match = TestSingle(input);
                if (match)
                {
                    return true;
                }
            }

            LastMatch = null;
            return false;
         }

        private bool TestSingle(WordProcessorResult wpr)
        {
            if (wpr.Count < 3) {
                return false;
            }

            if (wpr.Count == 3)
            {
                var success = TestWordTriple(wpr.Words[0], wpr.Words[1], wpr.Words[2]);

                if (success)
                {
                    LastMatch = new Name(wpr.Words[0], wpr.Words[1], wpr.Words[2]);
                    Log.Information("Viable Name Detected: {0}", LastMatch);
                    return true;
                }
            }

            if (wpr.Count > 3)
            {
                var allWombos = wpr.GetSubcollectionOfLengthThreeAnyPermutation();
                return TestForPerfectMatch(allWombos);
            }

            return false;

        }

        private bool TestWordTriple(string w1, string w2, string w3)
        {
            var w1Lower = w1.ToLower();
            var w2Lower = w2.ToLower();
            var w3Lower = w3.ToLower();

            var p1 = PossibleNames.FirstNames(true);
            var p2 = PossibleNames.SecondNames(true);
            var p3 = PossibleNames.ThirdNames(true);

            return p1.Contains(w1Lower) && p2.Contains(w2Lower) && p3.Contains(w3Lower);
        }
    }
}
