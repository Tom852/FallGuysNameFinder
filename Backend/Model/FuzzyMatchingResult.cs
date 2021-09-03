using Common.Model;
using Common.Util;
using System;
using System.Linq;
using System.Text;

namespace Backend.Model
{
    public class FuzzyMatchingResult
    {
        const float clearWinnerDicidingFactor = 1.5f;

        public Scoreboard<string> First { get; set; } = new Scoreboard<string>();
        public Scoreboard<string> Second { get; set; } = new Scoreboard<string>();
        public Scoreboard<string> Third { get; set; } = new Scoreboard<string>();

        public bool HasClearResult()
        {
            var ic1 = First.IsClear(clearWinnerDicidingFactor);
            var ic2 = Second.IsClear(clearWinnerDicidingFactor);
            var ic3 = Third.IsClear(clearWinnerDicidingFactor);
            return ic1 && ic2 && ic3;
        }

        public bool HasAnyResult()
        {
            var hw1 = First.HasWinner();
            var hw2 = First.HasWinner();
            var hw3 = First.HasWinner();
            return hw1 && hw2 && hw3;
        }

        public Name GetResult()
        {
            if (!HasAnyResult())
            {
                throw new InvalidOperationException("Cannot build a name out of an unsuccessful matching process.");
            }
            return new Name(First.GetWinner(), Second.GetWinner(), Third.GetWinner());
        }

        public override string ToString()
        {
            try
            {
                return GetResult().ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetReport()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Fuzzy Matching Report:");
            b.AppendLine();
            b.AppendLine("First Word");
            b.AppendLine("==========");
            b.AppendLine(First.ToString());
            b.AppendLine();
            b.AppendLine("Second Word");
            b.AppendLine("===========");
            b.AppendLine(Second.ToString());
            b.AppendLine();
            b.AppendLine("Third Word");
            b.AppendLine("==========");
            b.AppendLine(Third.ToString());

            return b.ToString();
        }
    }
}