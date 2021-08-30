using Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                FirstNames.AddOrIncrease(node.First);
                SecondNames.AddOrIncrease(node.Second);
                ThirdNames.AddOrIncrease(node.Third);
            }
        }
    }
}
