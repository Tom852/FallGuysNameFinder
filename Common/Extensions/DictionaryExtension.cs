using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
