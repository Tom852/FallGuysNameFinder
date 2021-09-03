using Backend.Model;
using Common.Util;
using FuzzySharp;
using FuzzySharp.Extractor;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
    public class FuzzyMatcher
    {
        private const int CUTOFF_LIMIT = 50;

        public FuzzyMatchingResult Result { get; private set; } 

        private void ClearOutputVariables()
        {
            Result = new FuzzyMatchingResult();
        }

        public bool Match(List<WordProcessorResult> inputs)
        {
            ClearOutputVariables();
            List<StringTriple> workItems = PrepareToWorkCollection(inputs);

            if (!workItems.Any())
            {
                Log.Warning("Fuzzy matcher has nothing to work with.");
                return false;
            }

            LogAvailableWorkItems(workItems);

            foreach (var item in workItems)
            {
                DoFuzzyComparison(item);
            }

            Log.Debug(Result.GetReport());

            if (!Result.HasAnyResult())
            {
                Log.Warning("Fuzzy Matching delivered no result.");
                return false;
            }
            if (!Result.HasClearResult())
            {
                // todo: there could be an option, stop on uncertainty or such. behaviour here could be revisited.
                // note: turns out it is basically never clear, even not with factor 1.5
                Log.Warning("Fuzzy matching has a result, but it is not very certain.");
            }

            Log.Information("Fuzzy matching succeeded. The result is: {0}", Result);
            return true;
        }

        private void LogAvailableWorkItems(List<StringTriple> inputs)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;

            StringBuilder b = new StringBuilder();
            b.AppendLine("The following string triples are availble");
            inputs.ForEach(f => b.AppendLine(f.ToString()));

            Log.Debug(b.ToString());

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void DoFuzzyComparison(StringTriple tripe)
        {
            var fuzzyMatches1 = DoFuzzyComparison(tripe.First, PossibleNames.FirstNames(false));
            var fuzzyMatches2 = DoFuzzyComparison(tripe.Second, PossibleNames.SecondNames(false));
            var fuzzyMatches3 = DoFuzzyComparison(tripe.Third, PossibleNames.ThirdNames(false));

            foreach (var fuzzyMatch in fuzzyMatches1)
            {
                Result.First.Add(fuzzyMatch.Value, fuzzyMatch.Score);
            }

            foreach (var fuzzyMatch in fuzzyMatches2)
            {
                Result.Second.Add(fuzzyMatch.Value, fuzzyMatch.Score);
            }

            foreach (var fuzzyMatch in fuzzyMatches3)
            {
                Result.Third.Add(fuzzyMatch.Value, fuzzyMatch.Score);
            }
        }

        private IEnumerable<ExtractedResult<string>> DoFuzzyComparison(string word, string[] nameOptions)
        {
            return Process.ExtractTop(word, nameOptions, s => s, cutoff: CUTOFF_LIMIT);

        }

        private StringTriple DetectMostLikelyPermutation(WordProcessorResult wpr)
        {
            Scoreboard<StringTriple> scoreboard = new Scoreboard<StringTriple>();
            var triples = wpr.GetSubcollectionOfLengthThreeAnyOrderedCombination();

            foreach (var triple in triples)
            {
                var result1 = Process.ExtractOne(triple.First, PossibleNames.FirstNames(false), s => s);
                var result2 = Process.ExtractOne(triple.Second, PossibleNames.SecondNames(false), s => s);
                var result3 = Process.ExtractOne(triple.Third, PossibleNames.ThirdNames(false), s => s);
                var totalScoreOfTriple = result1.Score + result2.Score + result3.Score;
                scoreboard.Add(triple, totalScoreOfTriple);
            }

            var result = scoreboard.GetWinner();
            Log.Debug("Best Fitting Triple of {0} is {1}", wpr.Words, result);
            return result;
        }

        private List<StringTriple> PrepareToWorkCollection(List<WordProcessorResult> inputs)
        {
            List<StringTriple> toWork = new List<StringTriple>();

            foreach (var wpr in inputs)
            {
                if (wpr.Count > 3)
                {
                    var t = DetectMostLikelyPermutation(wpr);
                    toWork.Add(t);
                }

                if (wpr.Count == 3)
                {
                    toWork.Add(wpr.GetAsTriple());
                }
            }

            toWork = toWork.Distinct().ToList();

            return toWork;
        }
    }
}