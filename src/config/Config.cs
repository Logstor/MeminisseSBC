using System;
using System.IO;

namespace Meminisse
{
    public class Config
    {
        // Fields
        public static string COBODPath { get; set; } = "../../../sd/sys/COBOD/";
        public static string GeneralPath { get; set; } = Path.Combine(COBODPath,"Meminisse/");
        public static string ConfigurationPath { get; set; } = Path.Combine(GeneralPath, "");
        public static string ConfigurationFilename { get; set; } = "MeminisseConfig.json";
        public static string ConfigurationFullPath { get; set; } = Path.Combine(ConfigurationPath, ConfigurationFilename);
        public static string DataPath { get; set; } = Path.Combine(GeneralPath, "data");

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
        /// How many times should we check if a print is started, when we're in idle state?
        /// </summary>
        /// <value></value>
        public int IdleCheckPrMin { get; set; } = 15;

        /// <summary>
        /// Should we log while the printer state is paused?
        /// </summary>
        /// <value></value>
        public bool LogWhilePaused { get; set; } = false;

        /// <summary>
        /// Should position be logged?
        /// </summary>
        /// <value></value>
        public bool LogPosition { get; set; } = true;

        /// <summary>
        /// Should Speed be logged?
        /// </summary>
        /// <value></value>
        public bool LogPrintSpeed { get; set; } = true;

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

        /// <summary>
        /// Should Main board voltages be logged?
        /// </summary>
        /// <value></value>
        public bool LogVoltages { get; set; } = true;

        private static DateTime configLastModification;

        private static Config _instance;
        public static Config instance { 
            get 
            {
                if (_instance == null)
                {
                    (_instance, configLastModification) = Configuration.Loader.Load(ConfigurationFullPath);
                }
                return _instance;
            } 
            private set { _instance = value; } }

        internal Config() {}

        /// <summary>
        /// Refreshes the configuration instance if there has been a change to the configuration file since last 
        /// refresh or creation.
        /// </summary>
        /// <returns>True if it refreshed</returns>
        public bool Refresh()
        {
            int comparison = File.GetLastWriteTimeUtc(ConfigurationFullPath).CompareTo(configLastModification);
            if (comparison > 0)
            {
                Logger.instance.I("Refreshing Configuration");
                this._Refresh();
                return true;
            }
            return false;
        }

        public void ForceRefresh()
        {
            Logger.instance.I("Forcing refresh of Configuration");
            this._Refresh();
        }

        private void _Refresh()
        {
            // Refresh the configuration
            (_instance, configLastModification) = Configuration.Loader.Load(ConfigurationFullPath);

            // Update logger
            Logger.instance.ChangeLogLevel(_instance.ConsoleLogLevel);
        }
    }
}