using Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using System.Threading.Tasks;

namespace Backend
{
    public static class PossibleNames
    {
        private static string[] firsts;
        private static string[] seconds;
        private static string[] thirds;

        public static string[] FirstNames()
        {
            if (firsts == null)
            {
                firsts = new DataStorageStuff().GetNamePossibilities(1);
            }
            return firsts;
        }

        public static string[] SecondNames()
        {
            if (seconds == null)
            {
                seconds = new DataStorageStuff().GetNamePossibilities(2);
            }
            return seconds;
        }

        public static string[] ThirdNames()
        {
            if (thirds == null)
            {
                thirds = new DataStorageStuff().GetNamePossibilities(3);
            }
            return thirds;
        }
    }
}
