using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class History
    {
        private List<string[]> PreviousNames { get; set; } = new List<string[]>(); //todo: use name class instead string[], parsed name that can contain more than 4.

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

        public List<string[]> GetWithoutSameElementsInRow()
        {
            if (PreviousNames.Count < 2)
            {
                return PreviousNames;
            }

            var node = PreviousNames[1];
            var previous = PreviousNames.First();
            List<string[]> result = new List<string[]>() { previous };

            foreach (var item in PreviousNames.GetRange(1, PreviousNames.Count - 1))
            {
                node = item;
                if (node[0] != previous[0] || node[1] != previous[1] || node[2] != previous[2])
                {
                    result.Add(node);
                }
                previous = node;
            }
            return result;
        }

    }
}
