using System.IO;

namespace Meminisse
{
    public class Config
    {
        // Fields
        public static string ConfigurationPath { get; set; } = "../conf/";
        public static string ConfigurationFilename { get; set; } = "config.json";
        public LogLevel LogLevel { get; set; } = LogLevel.INFO;
        public bool LogPosition { get; set; } = true;
        public bool LogSpeed { get; set; } = true;
        public bool LogTime { get; set; } = true;
        public bool LogExtrusion { get; set; } = true;
        public bool LogLayer { get; set; } = true;
        public bool LogBaby { get; set; } = true;

        private static Config _instance;
        public static Config instance { 
            get 
            {
                if (_instance == null)
                {
                    _instance = Configuration.Loader.Load(Path.Combine(ConfigurationPath, ConfigurationFilename));
                }
                return _instance;
            } 
            private set { _instance = value; } }

        internal Config() {}
    }
}