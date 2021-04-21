using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Meminisse.Configuration
{
    internal static class Loader
    {
        internal static Config Load(string fullPath)
        {
            try 
            {
                string json = File.ReadAllText(fullPath);
                return JsonConvert.DeserializeObject<Config>(json);
            }
            catch (DirectoryNotFoundException)    
            { 
                return CreateEverything(fullPath); 
            }
            catch (FileNotFoundException)         
            { 
                return CreateEverything(fullPath); 
            }
        }

        private static Config CreateEverything(string fullPath)
        {
            Config config = new Config();
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);

            // Make sure to create Directories
            Directory.CreateDirectory(Config.ConfigurationPath);

            // Write to config file
            File.WriteAllTextAsync(fullPath, json, Encoding.UTF8);

            return config;
        }
    }
}