using Backend.Model;
using FuzzySharp;
using FuzzySharp.Extractor;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend
{
    public class FuzzyMatcher
    {
        private const int CUTOFF_LIMIT = 50;

        public FuzzyMatcherResult Result { get; private set; } 

        private void ClearOutputVariables()
        {
            Result = new FuzzyMatcherResult();
        }

        public bool Match(List<WordProcessorResult> inputs)
        {
            ClearOutputVariables();
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
            ExtractedResult<string> fuzzyResult;
            fuzzyResult = DoFuzzyComparison(tripe.First, PossibleNames.FirstNames(false));
            if (fuzzyResult != null && fuzzyResult.Score > this.Result.First.Score)
            {
                Log.Debug("Fuzzy Matching Sucess: Position {0} - Input {1} - Matching {2} - Score {3}", 1, tripe.First, fuzzyResult.Value, fuzzyResult.Score);
                Result.First = new SingleFuzzyResult(fuzzyResult.Value, fuzzyResult.Score);
            }

            fuzzyResult = DoFuzzyComparison(tripe.Second, PossibleNames.SecondNames(false));
            if (fuzzyResult != null && fuzzyResult.Score > this.Result.Second.Score)
            {
                Log.Debug("Fuzzy Matching Sucess: Position {0} - Input {1} - Matching {2} - Score {3}", 2, tripe.Second, fuzzyResult.Value, fuzzyResult.Score);
                Result.Second = new SingleFuzzyResult(fuzzyResult.Value, fuzzyResult.Score);
            }

            fuzzyResult = DoFuzzyComparison(tripe.Third, PossibleNames.ThirdNames(false));
            if (fuzzyResult != null && fuzzyResult.Score > this.Result.Third.Score)
            {
                Log.Debug("Fuzzy Matching Sucess: Position {0} - Input {1} - Matching {2} - Score {3}", 3, tripe.Third, fuzzyResult.Value, fuzzyResult.Score);
                Result.Third = new SingleFuzzyResult(fuzzyResult.Value, fuzzyResult.Score);
            }
        }

        private ExtractedResult<string> DoFuzzyComparison(string word, string[] nameOptions)
        {
            return Process.ExtractOne(word, nameOptions, s => s, cutoff: CUTOFF_LIMIT);

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