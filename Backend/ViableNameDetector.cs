using Backend.Model;
using Common;
using Common.Model;
using Serilog;
using System;
using System.Linq;

namespace Backend
{
    public class ViableNameDetector
    {
        public Name LastMatch { get; set; }

        private void ClearOutputVariables()
        {
            LastMatch = default;
        }

        public bool TestForViableName(params WordProcessorResult[] inputs)
        {
            ClearOutputVariables();

            foreach (var input in inputs)
            {
                var fits = TestSingle(input);
                if (fits)
                {
                    return true;
                }
            }

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

            var possibilities1Lower = PossibleNames.FirstNames(true).ToList();
            var possibilities2Lower = PossibleNames.SecondNames(true).ToList();
            var possibilities3Lower = PossibleNames.ThirdNames(true).ToList();

            var isViable = possibilities1Lower.Contains(w1Lower) && possibilities2Lower.Contains(w2Lower) && possibilities3Lower.Contains(w3Lower);

            if (isViable)
            {
                var possibilities1CorrectCase = PossibleNames.FirstNames(false).ToList();
                var possibilities2CorrectCase = PossibleNames.SecondNames(false).ToList();
                var possibilities3CorrectCase = PossibleNames.ThirdNames(false).ToList();

                var word1index = possibilities1Lower.IndexOf(w1Lower);
                var word2index = possibilities1Lower.IndexOf(w2Lower);
                var word3index = possibilities1Lower.IndexOf(w3Lower);

                var w1Cleaned = possibilities1CorrectCase.ElementAt(word1index);
                var w2Cleaned = possibilities2CorrectCase.ElementAt(word2index);
                var w3Cleaned = possibilities3CorrectCase.ElementAt(word3index);

                // todo: this belongs into a test... speaking of tests *caugh*
                if (w1Cleaned.ToLower() != w1Lower || w2Cleaned.ToLower() != w2Lower || w3Cleaned.ToLower() != w3Lower)
                {
                    throw new Exception("case-cleaned words don't match original parsed names");
                }

                LastMatch = new Name(w1Cleaned, w2Cleaned, w3Cleaned);
            }

            return isViable;
        }
    }
}