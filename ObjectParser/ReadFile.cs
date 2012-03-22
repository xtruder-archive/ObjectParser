using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ObjectParser
{
    class ReadFile
    {
        public static string ReadFileAsString(string filename)
        {
            StreamReader Reader = File.OpenText(filename);
            if (Reader == null)
                return null;

            return Reader.ReadToEnd();
        }

        public static void WriteStringToFile( string filename, string data )
        {
            File.WriteAllText(filename, data);
        }
    }
}
