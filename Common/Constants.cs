namespace Common
{
    public static class Constants
    {
        public const int FailInRowLimit = 10;

        public const int TimeBeforeStart = 5000;
        public const int TiemAtEndBeforePressingEsc = 500;
        public const int TimeWaitAfterSpace = 3000;

        public const int TimeWaitWhenFgNotForeground = 5000;

        public const int TimeBetweenPresses = 2500;
        public const int TimeVariationBetweenPresses = 300;
        public const int TimePerHitOnAverageForStatistics = 3000;

        public const int LogFileSizeLimit = 50 * 1024 * 1024;
        public const int LogFilesToKeep = 10;

        public const float FuzzyMatchingClearDifferenceFaktor = 1.5f;
    }
}