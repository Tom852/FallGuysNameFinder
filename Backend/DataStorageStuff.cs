using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class DataStorageStuff
    {
        const string Appname = "FallGuysNameFinder";
        const string FileName = "names.txt";

        private string file;

        public DataStorageStuff()
        {
            CreateFileIfNotExists();
        }

        public List<Name> Read()
        {
            var lines = File.ReadAllLines(file);
            List<Name> result = new List<Name>();

            foreach (var line in lines)
            {
                try
                {
                    result.Add(new Name(line));
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error while reading names-file. Item Skipped.");
                }
            }
            return result;
        }

        public void AddLine(string line)
        {
            File.AppendAllLines(file, new List<string>() { line });
        }

        private void CreateFileIfNotExists()
        {
            var env = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(env, Appname);
            var file = Path.Combine(env, Appname, FileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!File.Exists(file))
            {
                var stream = File.Create(file);
                stream.Close();
            }
            this.file = file;
        }

    }
}
