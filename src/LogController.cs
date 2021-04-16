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

        private ILogController logFileControl = new CSVLogController();

        /// <summary>
        /// Creates an instance of the logging controller.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="initialState"></param>
        public LogController(Logger logger)
        {
            this.logger = logger;
            this.dataAccess = CodeDataAccess.getInstance(this.logger);
        }

        /// <summary>
        /// Starts the logging loop.
        /// </summary>
        /// <param name="updateFreq">How many updates per second</param>
        public async Task start(int updateFreq)
        {
            // Calculate delay
            long delayms = 1000L / (long)updateFreq;

            // Initialize LogFileController
            List<LogEntity> list = new List<LogEntity>(1);
            list.Add(LogEntity.Position);
            this.logFileControl.Init(await this.GetLogFilename(), list);
            await MainLoop(delayms);
        }

        private async Task MainLoop(long delayms)
        {
            // Setup timing
            Stopwatch totalTime = new();
            totalTime.Start();
            Stopwatch logTimer = new();

            // Loop until Job is done or the machine is turned off
            MachineStatus status;
            Position positionEntity;
            do
            {
                logTimer.Restart();

                positionEntity = await this.dataAccess.requestPosition();
                status = await this.dataAccess.requestStatus();

                // If prints is paused
                if (status == MachineStatus.Paused || status == MachineStatus.Pausing)
                {
                    this.logger.T("Machine Paused");
                    Thread.Sleep((int)delayms);
                    continue;
                }

                // If print is done or halted then break
                else if (status == MachineStatus.Idle || status == MachineStatus.Off || status == MachineStatus.Halted)
                {
                    this.logger.T("Machine Idle");
                    break;
                }

                // Write to log
                List<ILogEntity> list = new List<ILogEntity>(1);
                list.Add(positionEntity);
                this.logFileControl.Add(totalTime.ElapsedMilliseconds, list);
                this.logFileControl.FlushToFile();

                logTimer.Stop();
                this.logger.T(string.Format("Log took {0} milliseconds", logTimer.ElapsedMilliseconds));

                // Wait if we're before time
                if (!(logTimer.ElapsedMilliseconds >= delayms))
                    Thread.Sleep((int)(delayms - logTimer.ElapsedMilliseconds));

                // Otherwise warn the we can't reach the desired frequency
                else
                    this.logger.W(
                        string.Format("We can't log this fast! Current log time: {0} ms - Max log time: {1} ms",
                            logTimer.ElapsedMilliseconds, delayms));
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
    }
}

