using Common;
using System;

namespace Backend.Model
{
    public struct Probability
    {
        public double Value { get; }
        public double Percentage => 100 * Value;

        public double Promille => 1000 * Value;

        public Probability(double raw)
        {
            this.Value = raw;
        }

        public string GetProbabilityAsFormattedString()
        {
            if (Value == 0)
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
            if (Value == 0)
            {
                return "N/A";
            }

            var attempts = 1d / Value;
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