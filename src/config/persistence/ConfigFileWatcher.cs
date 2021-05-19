using System;
using System.IO;

namespace Meminisse
{
    /// <summary>
    /// A FileSystemWatcher that watches the Configuration file for changes.
    /// </summary>
    public class ConfigFileWatcher : IDisposable
    {
        private FileSystemWatcher watcher;

        private NotifyFilters filters = NotifyFilters.LastWrite | NotifyFilters.CreationTime;

        /// <summary>
        /// Enable or Disable the watcher.
        /// 
        /// This is used when you just want to disable the watcher, without disposing of the object.
        /// </summary>
        /// <value>True or False</value>
        public bool Enabled 
        {
            get { return this.watcher.EnableRaisingEvents; }
            set { this.watcher.EnableRaisingEvents = value; }
        }

        public ConfigFileWatcher()
        {
            this.watcher = new FileSystemWatcher(Config.ConfigurationPath);

            // Setup
            this.watcher.NotifyFilter           = this.filters;
            this.watcher.IncludeSubdirectories  = false;
            this.watcher.Filter                 = Config.ConfigurationFilename;
        }

        /// <summary>
        /// Adds a handler for changes to the configuration file.
        /// </summary>
        /// <param name="handler">FileSystemEventHandler</param>
        public void AddOnChangedHandler(FileSystemEventHandler handler)
        {
            this.watcher.Changed += handler;
        }
        
        public void Dispose()
        {
            watcher.Dispose();
        }
    }
}