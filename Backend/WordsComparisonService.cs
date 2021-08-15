using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Tesseract;
using Serilog;

namespace Backend
{
    public class WordsComparisonService
    {

        public WordsComparisonService(DataStorageStuff storageAccess)
        {
            this.AllowedNames = storageAccess.Read();
        }

        public WordsComparisonService(List<Name> allowedNames)
        {
            AllowedNames = allowedNames;
        }

        public List<Name> AllowedNames { get; }

        public bool Test(string s)
        {

            var nameToTest = new Name(s);

            return this.AllowedNames.Any(goodName => Matches(goodName, nameToTest));


        }

        private bool Matches(Name pattern, Name toTest)
        {


            return (pattern.First == "*" || toTest.First == pattern.First) &&
                (pattern.Second == "*" || toTest.Second == pattern.Second) &&
                (pattern.Third == "*" || toTest.Third == pattern.Third);
        }

    }
}
