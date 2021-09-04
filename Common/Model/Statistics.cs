using Common.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Common.Model
{
    public class Statistics
    {
        private Dictionary<string, int> firstNames = new Dictionary<string, int>();
        private Dictionary<string, int> secondNames = new Dictionary<string, int>();
        private Dictionary<string, int> thirdNames = new Dictionary<string, int>();

        public Dictionary<string, int> FirstNames {
            get => firstNames.OrderBy(s => s.Key).ToDictionary(k => k.Key, k => k.Value);
            set => firstNames = value;
        }
        public Dictionary<string, int> SecondNames {
            get => secondNames.OrderBy(s => s.Key).ToDictionary(k => k.Key, v=>v.Value);
            set => secondNames = value;
        }
        public Dictionary<string, int> ThirdNames {
            get => thirdNames.OrderBy(s => s.Key).ToDictionary(k => k.Key, k => k.Value);
            set => thirdNames = value;
        }

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