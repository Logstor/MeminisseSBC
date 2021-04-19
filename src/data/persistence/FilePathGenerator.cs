using System;
using System.IO;

namespace Meminisse
{
    public static class FilePathGenerator
    {
        static string DataLogPath = "../../../sd/Meminisse/data/";

        /// <summary>
        /// Appending the current Month and Day to the back of the given filename.
        /// 
        /// Year-Month-Day-Time--filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>string with appended month and day</returns>
        public static string AppendTimeStamp(string filename)
        {
            DateTime date = DateTime.Now;
            string year = date.Year.ToString("D4");
            string month = date.Month.ToString("D2");
            string day = date.Day.ToString("D2");
            string time = string.Format("{0:D2}{1:D2}", date.Hour, date.Minute);

            return $"{year}-{month}-{day}-{time}--{filename}";
        }

        /// <summary>
        /// Appends the path to where the log should be placed. It makes sure the logfile (filename) is placed 
        /// correctly so it conforms to the chosen data log structure.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>string with the whole path ending on the given filename</returns>
        public static string AppendDataLogPath(string filename)
        {
            return GetDataLogPath() + filename;
        }

        public static string GetDataLogPath() { return DataLogPath + DateTime.Now.Year.ToString("D4") + "/"; }
    }
}