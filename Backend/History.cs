using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class History
    {
        private List<Name> PreviousNames { get; set; } = new List<Name>();

        public bool WereLastNamesAllEqual(int i)
        {
            if (PreviousNames.Count < i)
            {
                return false;
            }

            var lastElement = PreviousNames.Last();

            return PreviousNames.GetRange(PreviousNames.Count - i, i).All(node => node == lastElement);
        }

        public void Add(Name node)
        {
            this.PreviousNames.Add(node);
        }

        public List<Name> GetWithoutSameElementsInRow()
        {
            if (PreviousNames.Count < 2)
            {
                return PreviousNames;
            }

            var current = PreviousNames[1];
            var previous = PreviousNames.First();
            List<Name> result = new List<Name>() { previous };

            foreach (var item in PreviousNames.GetRange(1, PreviousNames.Count - 1))
            {
                current = item;
                if (current != previous)
                {
                    result.Add(current);
                }
                previous = current;
            }
            return result;
        }

    }
}
