namespace Backend.Model
{
    public enum MatchingResult
    {
        NoMatch,
        Alliteration,
        DoubleWord,
        Pattern,
        Pool
    }

    public static class MatchingResultExtension
    {
        public static bool IsMatching(this MatchingResult r)
        {
            return r != MatchingResult.NoMatch;
        }
    }
}