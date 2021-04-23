using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using System.Collections.Generic;

using DuetAPIClient;
using DuetAPI;
using DuetAPI.Connection;
using DuetAPI.ObjectModel;

namespace Meminisse 
{
    /// <summary>
    /// Log controller that is responsible for logging a single job
    /// </summary>
    public class LogController
    {
        private IDataAccess dataAccess;

        private Logger logger;

        private long logDelayMs = 60000L / Config.instance.LogsPrMin;

        private ILogController logFileControl = new CSVLogController();

        /// <summary>
        /// Creates an instance of the logging controller.
        /// </summary>
        public LogController(Logger logger)
        {
            this.logger = logger;
            this.dataAccess = CodeDataAccess.getInstance(this.logger);
        }

        /// <summary>
        /// Starts the logging loop.
        /// </summary>
        public async Task start()
        {
            this.logger.D(string.Format("Starting logging with {0} milliseconds delay", this.logDelayMs));

            // Initialize LogFileController
            this.logger.T("Initializing LogController");
            this.logFileControl.Init(await this.GetLogFilename(), CreateInitLogList());
            this.logger.T("Starting MainLoop");
            await MainLoop();
        }

        private async Task MainLoop()
        {
            // Setup timing
            Stopwatch totalTime = new();
            totalTime.Start();
            Stopwatch logTimer = new();

            // Loop until Job is done or the machine is turned off
            EntityWrap entities;
            do
            {
                // Restart log timer and get full update
                logTimer.Restart();
                this.logger.T("Requesting full Data model");
                entities = await this.dataAccess.requestFull();

                // If prints is paused
                if (entities.machineStatus == MachineStatus.Paused || entities.machineStatus == MachineStatus.Pausing)
                {
                    this.logger.T("Machine Paused");
                    Thread.Sleep((int)this.logDelayMs);
                    continue;
                }

                // If print is done or halted then break
                else if (entities.machineStatus == MachineStatus.Idle || entities.machineStatus == MachineStatus.Off || entities.machineStatus == MachineStatus.Halted)
                {
                    this.logger.T("Machine Idle");
                    break;
                }

                // Write to log
                this.logFileControl.Add(totalTime.ElapsedMilliseconds, CreateLogList(entities));
                this.logFileControl.FlushToFile();

                logTimer.Stop();
                this.logger.T(string.Format("Log took {0} milliseconds", logTimer.ElapsedMilliseconds));

                // Wait if we're before time
                if (! (logTimer.ElapsedMilliseconds >= this.logDelayMs) )
                    Thread.Sleep((int)(this.logDelayMs - logTimer.ElapsedMilliseconds));

                // Otherwise warn the we can't reach the desired frequency
                else
                    this.logger.W(
                        string.Format("We can't log this fast! Current log time: {0} ms - Max log time: {1} ms",
                            logTimer.ElapsedMilliseconds, this.logDelayMs));
            }
            while (true);
        }

        private async Task<string> GetLogFilename()
        {
            // Retrieve
            string path = await this.dataAccess.requestCurrentFilePath();

            // Parse
            string filename = Path.GetFileNameWithoutExtension(path);
            filename = filename.Trim();

            // Add extension
            return filename + ".log";
        }

        private List<LogEntity> CreateInitLogList()
        {
            List<LogEntity> list = new List<LogEntity>(10);

            if (Config.instance.LogPosition)
                list.Add(LogEntity.Position);

            if (Config.instance.LogSpeed)
                list.Add(LogEntity.Speed);
            
            if (Config.instance.LogTime)
                list.Add(LogEntity.Time);
            
            if (Config.instance.LogExtrusion)
                list.Add(LogEntity.Extrusion);
            
            if (Config.instance.LogBaby)
                list.Add(LogEntity.Babystep);
            
            return list;
        }

        private List<ILogEntity> CreateLogList(EntityWrap entities)
        {
            List<ILogEntity> list = new List<ILogEntity>(10);

            if (Config.instance.LogPosition)
                list.Add(entities.position);

            if (Config.instance.LogSpeed)
                list.Add(entities.speed);

            if (Config.instance.LogTime)
                list.Add(entities.time);

            if (Config.instance.LogExtrusion)
                list.Add(entities.extrusion);

            if (Config.instance.LogBaby)
                list.Add(entities.babystep);

            return list;
        }
    }
}

