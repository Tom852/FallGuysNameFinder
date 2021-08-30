using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class FuzzyMatcherResult
    {
        public SingleFuzzyResult First { get; set; }
        public SingleFuzzyResult Second { get; set; }
        public SingleFuzzyResult Third { get; set; }

        public bool IsSuccess()
        {
            return First.Score != 0 && Second.Score != 0 && Third.Score != 0;
        }

        public Name GetAsName()
        {
            if (!IsSuccess())
            {
                throw new InvalidOperationException("Cannot build a name out of an unsuccessful matching process.");
            }
            return new Name(First.Word, Second.Word, Third.Word);
        }

        public override string ToString()
        {
            return $"{First.Word} ({First.Score} Pts) | {Second.Word} ({Second.Score} Pts) | {Third.Word} ({Third.Score} Pts)";
        }
    }

    public struct SingleFuzzyResult
    {
        public SingleFuzzyResult(string word, int score) => (Word, Score) = (word, score);
        public string Word { get; set; }
        public int Score { get; set; }
    }
}
