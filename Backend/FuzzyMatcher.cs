using Common;
using Common.Model;
using FuzzySharp;
using FuzzySharp.Extractor;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend
{
    public class FuzzyMatcher
    {
        const int CUTOFF_LIMIT = 50;

        public FuzzyMatcherResult Result { get; private set; }

        public bool Match(List<WordProcessorResult> inputs)
        {


            List<StringTriple> workItems = PrepareToWorkCollection(inputs);

            ConsolerPrintState(workItems); // bit for debugging, could remove.


            foreach (var item in workItems)
            {
                DoFuzzyComparison(item);
            }

            if (!Result.IsSuccess())
            {
                Log.Warning("Fuzzy matching failed.");
                return false;
            }

            Log.Information("Fuzzy matching succeeded. The result is: {0}", Result);
            return true;
        }

        private void ConsolerPrintState(List<StringTriple> inputs)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkBlue;

            Console.WriteLine("The follwoing string Triples are availble");
            inputs.ForEach(f => Console.WriteLine(f));

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void DoFuzzyComparison(StringTriple tripe)
        {
            var result1 = DoFuzzyComparison(tripe.First, PossibleNames.FirstNames(false));
            if (result1.Score > this.Result.First.Score)
            {
                Log.Debug("New best-match for Position 1.");
                Result.First = new SingleFuzzyResult(result1.Value, result1.Score);
            }

            var result2 = DoFuzzyComparison(tripe.Second, PossibleNames.SecondNames(false));
            if (result2.Score > this.Result.Second.Score)
            {
                Log.Debug("New best-match for Position 2.");
                Result.Second = new SingleFuzzyResult(result2.Value, result2.Score);
            }

            var result3 = DoFuzzyComparison(tripe.Third, PossibleNames.ThirdNames(false));
            if (result3.Score > this.Result.Third.Score)
            {
                Log.Debug("New best-match for Position 3.");
                Result.Third = new SingleFuzzyResult(result3.Value, result3.Score);
            }
        }

        private ExtractedResult<string> DoFuzzyComparison(string word, string[] nameOptions)
        {
            var r = Process.ExtractOne(word, nameOptions, s => s, cutoff: CUTOFF_LIMIT);

            if (r != null)
            {
                Log.Debug("Fuzzy Matching Sucess: Input {0} - Matching {1} - Score {2}", word, r.Value, r.Score);
            }
            else
            {
                Log.Debug("{0} could not be matched", word);
            }
            return r;
        }

        private StringTriple DetectMostLikelyPermutation(WordProcessorResult wpr)
        {
            Dictionary<StringTriple, int> scoreBoard = new Dictionary<StringTriple, int>();
            var triples = wpr.GetSubcollectionOfLengthThreeAnyOrderedCombination();

            foreach (var triple in triples)
            {
                var result1 = Process.ExtractOne(triple.First, PossibleNames.FirstNames(false), s => s);
                var result2 = Process.ExtractOne(triple.Second, PossibleNames.SecondNames(false), s => s);
                var result3 = Process.ExtractOne(triple.Third, PossibleNames.ThirdNames(false), s => s);
                scoreBoard.Add(triple, result1.Score + result2.Score + result3.Score);
            }

            var maxValue = scoreBoard.Values.Max();
            var result = scoreBoard.First(s => s.Value == maxValue).Key;
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