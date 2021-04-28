using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Meminisse.Configuration
{
    internal static class Loader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        internal static (Config, DateTime) Load(string fullPath)
        {
            try 
            {
                // Check if path and file exists
                if ( File.Exists(fullPath) )
                {
                    string json = File.ReadAllText(fullPath);
                    return ( JsonConvert.DeserializeObject<Config>(json), File.GetLastWriteTimeUtc(fullPath) );
                }
                else
                {
                   return ( CreateEverything(fullPath), File.GetLastWriteTimeUtc(fullPath) );
                }
            }
            catch (Exception e)    
            {
                Console.WriteLine("ERROR Loading or Creating configuration file!");
                Console.WriteLine(e.ToString());
                Console.WriteLine("Continuous with default configuration");
                return (new Config(), DateTime.UnixEpoch);
            }
        }

        private static Config CreateEverything(string fullPath)
        {
            Config config = new Config();
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);

            // Create Directories and File
            using FileStream fs = FileGenerator.CreateFileWithPermissions(fullPath);

            // Write to file
            byte[] array = Encoding.UTF8.GetBytes(json);
            fs.Write(array, 0, array.Length);

            return config;
        }
    }
}