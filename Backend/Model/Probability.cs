using Common;
using System;

namespace Backend.Model
{
    public struct Probability
    {

        public double MatchCount { get; }
        public double TotalCount { get; }
        public double ProbabilityValue { get; }
        public double Percentage { get; }
        public double Promille { get; }

        public Probability(double matchCount, double totalOptions)
        {
            this.MatchCount = matchCount;
            this.TotalCount = totalOptions;
            this.ProbabilityValue = matchCount / totalOptions;
            this.Percentage = 100 * this.ProbabilityValue;
            this.Promille = 1000 * this.ProbabilityValue;
        }

        public string GetCombinationsAsFormattedString()
        {
            return this.MatchCount.ToString("n0");
        }

        public string GetProbabilityAsFormattedString()
        {
            if (ProbabilityValue == 0)
            {
                return "0.00 %";
            }
            if (Promille < 0.01)
            {
                return "< 0.01 ‰";
            }
            if (Promille < 2)
            {
                return Promille.ToString("n2") + " ‰";
            }
            if (Percentage < 2)
            {
                return Percentage.ToString("n2") + " %";
            }
            if (Percentage < 10)
            {
                return Percentage.ToString("n1") + " %";
            }
            return Percentage.ToString("n0") + " %";
        }

        public string GetTimeRequired()
        {
            if (ProbabilityValue == 0)
            {
                return "N/A";
            }

            var attempts = 1d / ProbabilityValue;
            var timeRquired = TimeSpan.FromSeconds(attempts * Constants.TimePerHitOnAverageForStatistics / 1000);

            if (timeRquired.Days != 0)
            {
                return timeRquired.TotalDays.ToString("n1") + " d";
            }

            if (timeRquired.Hours != 0)
            {
                return timeRquired.TotalHours.ToString("n0") + " h";
            }

            if (timeRquired.Minutes != 0)
            {
                return timeRquired.TotalMinutes.ToString("n0") + " min";
            }

            return timeRquired.TotalSeconds.ToString("n0") + " sec";

        }
    }
}