using System.Collections.Generic;

namespace Common.Extensions
{
    public static class DictionaryExtension
    {
        public static void AddOrIncrease(this Dictionary<string, int> d, string key)
        {
            if (d.ContainsKey(key))
            {
                d[key]++;
            }
            else
            {
                d.Add(key, 1);
            }
        }
    }
}