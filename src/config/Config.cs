using System.IO;

namespace Meminisse
{
    public class Config
    {
        // Fields
        public static string ConfigurationPath { get; set; } = "../../../sd/sys/";
        public static string ConfigurationFilename { get; set; } = "MeminisseConfig.json";

        /// <summary>
        /// Minimum loglevel of the logger.
        /// </summary>
        /// <value></value>
        public LogLevel ConsoleLogLevel { get; set; } = LogLevel.INFO;

        /// <summary>
        /// How many times should we log per minute?
        /// </summary>
        /// <value></value>
        public int LogsPrMin { get; set; } = 60;

        /// <summary>
        /// Should position be logged?
        /// </summary>
        /// <value></value>
        public bool LogPosition { get; set; } = true;

        /// <summary>
        /// Should Speed be logged?
        /// </summary>
        /// <value></value>
        public bool LogSpeed { get; set; } = true;

        /// <summary>
        /// Should Duration and Time be logged?
        /// </summary>
        /// <value></value>
        public bool LogTime { get; set; } = true;

        /// <summary>
        /// Should Extrusion be logged?
        /// </summary>
        /// <value></value>
        public bool LogExtrusion { get; set; } = true;

        /// <summary>
        /// Should babystep be logged?
        /// </summary>
        /// <value></value>
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