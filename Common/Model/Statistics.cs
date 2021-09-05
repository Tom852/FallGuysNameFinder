using Common.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Common.Model
{
    public class Statistics
    {
        public Dictionary<string, int> FirstNames { get; private set; } = new Dictionary<string, int>();
        public Dictionary<string, int> SecondNames { get; private set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ThirdNames { get; private set; } = new Dictionary<string, int>();

        public void Account(List<Name> previousNames)
        {
            foreach (var node in previousNames)
            {
                FirstNames.AddOrIncrease(node.First, 1);
                SecondNames.AddOrIncrease(node.Second, 1);
                ThirdNames.AddOrIncrease(node.Third, 1);
            }
        }

        public void Sort()
        {
            FirstNames = FirstNames.OrderBy(s => s.Key).ToDictionary(k => k.Key, k => k.Value);
            SecondNames = SecondNames.OrderBy(s => s.Key).ToDictionary(k => k.Key, v => v.Value);
            ThirdNames = ThirdNames.OrderBy(s => s.Key).ToDictionary(k => k.Key, k => k.Value); ;
        }
    }
}