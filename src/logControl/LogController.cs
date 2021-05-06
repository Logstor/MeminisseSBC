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
    public class LogController : IStateController
    {
        private const string FileExtension = ".csv";

        private IDataAccess dataAccess;

        private long logDelayMs = 60000L / Config.instance.LogsPrMin;

        private IState currentState = new StateIdle();

        /// <summary>
        /// Creates an instance of the logging controller.
        /// </summary>
        public LogController(Logger logger)
        {
            this.dataAccess = CodeDataAccess.getInstance(Logger.instance);
        }

        /// <summary>
        /// Starts the logging loop.
        /// </summary>
        public async Task start()
        {
            Logger.instance.D(string.Format("Starting logging with {0} milliseconds intervals", this.logDelayMs));

            // Enter the state
            this.currentState.OnEnterState(this);
            
            await MainLoop();
        }

        /// <summary>
        /// Changes the current state, and makes sure OnEnter and OnExit is running correctly.
        /// </summary>
        /// <param name="newState">New IState to change to</param>
        void IStateController.ChangeState(IState newState)
        {
            this.currentState.OnExitState(this);
            this.currentState = newState;
            this.currentState.OnEnterState(this);
        }

        string IStateController.GetCurrentFilename()
        {
            // Retrieve
            Task<string> task = this.dataAccess.requestCurrentFilePath();
            task.Wait();

            // Parse
            string filename = Path.GetFileNameWithoutExtension(task.Result);
            filename = filename.Trim();

            // Add extension
            return filename + FileExtension;
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
                Logger.instance.T("Requesting full Data model");
                entities = await this.dataAccess.requestFull();

                // State handle
                this.currentState.HandleUpdate(this, totalTime.ElapsedMilliseconds, entities);

                // Check log time
                logTimer.Stop();
                Logger.instance.D(string.Format("Log took {0} milliseconds", logTimer.ElapsedMilliseconds));

                // Wait if we're before time
                if (! (logTimer.ElapsedMilliseconds >= this.logDelayMs) )
                    Thread.Sleep((int)(this.currentState.logDelay - logTimer.ElapsedMilliseconds));

                // Otherwise warn the we can't reach the desired frequency
                else
                    Logger.instance.W(
                        string.Format("We can't log this fast! Current log time: {0} ms - Max log time: {1} ms",
                            logTimer.ElapsedMilliseconds, this.logDelayMs));
            }
            while (true);
        }
    }
}

