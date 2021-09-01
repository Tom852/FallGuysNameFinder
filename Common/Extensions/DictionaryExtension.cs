using System.Collections.Generic;

namespace Common.Extensions
{
    public static class DictionaryExtension
    {

        public static void AddOrIncrease<T>(this Dictionary<T, int> d, T key, int value)
        {
            if (d.ContainsKey(key))
            {
                d[key] = d[key] + value;
            }
            else
            {
                d.Add(key, value);
            }
        }
    }
}