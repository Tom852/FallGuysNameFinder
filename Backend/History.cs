using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class History
    {
        public List<string[]> PreviousNames { get; set; } = new List<string[]>(); //todo: use name class instead string[], parsed name that can contain more than 4.

        public bool WereLastNamesAllEqual(int i)
        {
            if (PreviousNames.Count < i)
            {
                return false;
            }

            var lastElement = PreviousNames.Last();

            return PreviousNames.GetRange(PreviousNames.Count - i, i).All(node =>
                node[0] == lastElement[0] && node[1] == lastElement[1] && node[2] == lastElement[2]
            );
        }

        public void Add(string[] node)
        {
            this.PreviousNames.Add(node);
        }

    }
}
