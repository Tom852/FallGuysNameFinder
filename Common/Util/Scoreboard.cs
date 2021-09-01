using Common.Extensions;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Util
{
    public class Scoreboard<T>
    {
        public Dictionary<T, int> Scores { get; private set; } = new Dictionary<T, int>();

        public void Add(T key, int score)
        {
            Scores.AddOrIncrease(key, score);
        }

        // Note: If someone reuses this, including myself... should make something if there are multiple first places.
        public T GetWinner()
        {
            if (!HasWinner())
            {
                throw new InvalidOperationException("There is no winner. No entries are in the scoreboard.");
            }

            var maxValue = Scores.Values.Max();
            return Scores.First(s => s.Value == maxValue).Key;
        }

        public bool IsClear(float factor)
        {
            if (factor < 1)
            {
                throw new ArgumentException("Facot should be larger than 1");
            }
            if (Scores.Count() < 2)
            {
                return true;
            }

            var justScoresOrdered = Scores.Values.OrderByDescending(s => s).ToList();
            return justScoresOrdered[0] > factor * justScoresOrdered[1];
        }

        public bool HasWinner()
        {
            return Scores.Any();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var entry in Scores.OrderByDescending(d => d.Value))
            {
                builder.AppendLine($"{entry.Key}: {entry.Value} Pts");
            }
            return builder.ToString();
        }
    }
}