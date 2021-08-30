using Backend.Model;
using Common.Model;
using Serilog;
using System.Linq;

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

            LastMatch = default;
            return false;
        }

        private bool TestSingle(WordProcessorResult wpr)
        {
            if (wpr.Count < 3)
            {
                return false;
            }

            if (wpr.Count == 3)
            {
                return TestWordTriple(wpr.GetAsTriple());
            }

            if (wpr.Count > 3)
            {
                var allWombos = wpr.GetSubcollectionOfLengthThreeAnyOrderedCombination();
                return allWombos.Any(wombo => TestWordTriple(wombo));
            }

            return false;
        }

        private bool TestWordTriple(StringTriple s)
        {
            var w1Lower = s.First.ToLower();
            var w2Lower = s.Second.ToLower();
            var w3Lower = s.Third.ToLower();

            var p1 = PossibleNames.FirstNames(true);
            var p2 = PossibleNames.SecondNames(true);
            var p3 = PossibleNames.ThirdNames(true);

            var result = p1.Contains(w1Lower) && p2.Contains(w2Lower) && p3.Contains(w3Lower);

            if (result)
            {
                Log.Information("Viable Name Detected: {0}", LastMatch);
                LastMatch = new Name(s.First, s.Second, s.Third);
            }

            return result;
        }
    }
}