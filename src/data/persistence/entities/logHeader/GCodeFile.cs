using System;
using System.IO;
using System.Text;

namespace Meminisse
{
    public class GCodeFile
    {
        public string gCodePath { get; set; }

        public string gCodeHeader { get; set; }

        private GCodeFile() { }

        /// <summary>
        /// Build a GCodeFile instance. 
        /// Uses the gCodePath to find the file, and read in the header of the file.
        /// </summary>
        /// <param name="gCodePath">Virtual SD path "0:/{Something}"</param>
        /// <returns>GCodeFile instance</returns>
        public static GCodeFile BuildFromPath(string gCodePath)
        {
            Logger.instance.T("Building GCodeFile instance");

            // Create and set path
            GCodeFile gCodeFile = new();
            gCodeFile.gCodePath = gCodePath;

            // Read in GCode header
            gCodeFile.gCodeHeader = ReadGCodeHeader(FilePathGenerator.ConvertGCodePathToRelative(gCodePath));

            return gCodeFile;
        }

        private static string ReadGCodeHeader(string gCodePath)
        {
            const int INITIAL_CAPACITY = 256;

            // Open Stream
            using FileStream fs = File.OpenRead(gCodePath);
            using StreamReader sr = new StreamReader(fs, Encoding.UTF8);

            // Read line for line with ";"
            StringBuilder sb = new StringBuilder(INITIAL_CAPACITY);
            string currLine = sr.ReadLine();
            while (currLine[0] == ';')
            {
                // Append
                sb.Append(currLine);

                // Read next line
                currLine = sr.ReadLine();
            } 

            return sb.ToString();
        }
    }
}