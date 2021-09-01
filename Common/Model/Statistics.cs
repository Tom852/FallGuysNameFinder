using Common.Extensions;
using System.Collections.Generic;

namespace Common.Model
{
    public class Statistics
    {
        public Dictionary<string, int> FirstNames { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> SecondNames { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ThirdNames { get; set; } = new Dictionary<string, int>();

        public void Account(List<Name> previousNames)
        {
            foreach (var node in previousNames)
            {
                FirstNames.AddOrIncrease(node.First, 1);
                SecondNames.AddOrIncrease(node.Second, 1);
                ThirdNames.AddOrIncrease(node.Third, 1);
            }
        }
    }
}