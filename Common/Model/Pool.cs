using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Common.Model
{
    public class Pool
    {
        public ObservableCollection<string> First { get; } = new ObservableCollection<string>(); // todo: not sure if we need obs. coll.
        public ObservableCollection<string> Second { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Third { get; } = new ObservableCollection<string>();

        public void AddFirst(string name) => AddName(name, 1);
        public void AddSecond(string name) => AddName(name, 2);
        public void AddThird(string name) => AddName(name, 3);

        public void RemoveFirst(string name) => RemoveName(name, 1);
        public void RemoveSecond(string name) => RemoveName(name, 2);
        public void RemoveThird(string name) => RemoveName(name, 3);

        private void RemoveName(string name, int which)
        {
            ObservableCollection<string> property;

            switch (which)
            {
                case 1:
                    property = First;
                    break;
                case 2:
                    property = Second;
                    break;
                case 3:
                    property = Third;
                    break;
                default:
                    throw new Exception("which must be 1 2 3");
            }

            if (!property.Contains(name))
            {
                throw new InvalidOperationException($"{name} is not in the list");
            }

            property.Remove(name);
        }

        private void AddName(string name, int which)
        {
            string[] possibleNames;
            ObservableCollection<string> property;

            switch (which)
            {
                case 1:
                    possibleNames = PossibleNames.FirstNames(false);
                    property = First;
                    break;
                case 2:
                    possibleNames = PossibleNames.SecondNames(false);
                    property = Second;
                    break;
                case 3:
                    possibleNames = PossibleNames.ThirdNames(false);
                    property = Third;
                    break;
                default:
                    throw new Exception("which must be 1 2 3");
            }

            if (!possibleNames.Contains(name))
            {
                throw new ArgumentException($"{name} is not a possibility.");
            }

            if (property.Contains(name))
            {
                throw new InvalidOperationException($"{name} is already in the list");
            }

            property.Add(name);
        }



        public override string ToString()
        {
            return $"{First} {Second} {Third}";
        }
    }
}