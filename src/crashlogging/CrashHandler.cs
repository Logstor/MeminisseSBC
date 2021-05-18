using System;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Meminisse
{
    public class CrashHandler
    {
        private AppDomain appDomain = AppDomain.CurrentDomain;

        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes the CrashHandler.
        /// </summary>
        public CrashHandler(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource;

            // Handle uncaught exceptions
            appDomain.UnhandledException += new UnhandledExceptionEventHandler(UncaughtExceptionHandler);
            
            // Handle Cancel key press
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);

            // Handle parent process exiting
            appDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void UncaughtExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Logger.instance.D("Uncaught Exception");
            this.SetCancellationToken();

            // Get Exception
            Exception ex = (Exception) args.ExceptionObject;

            // Create log file
            Logger.instance.T("Creating Logfile");

            string path = Path.Combine(Config.GeneralPath, FilePathGenerator.AppendTimeStamp("crash.log"));
            using FileStream fs = FileGenerator.CreateFileWithPermissions(path);
            using StreamWriter writer = new StreamWriter(fs, Encoding.UTF8);

            // Create log message
            Logger.instance.T("Creating Crash Log message");

            StringBuilder sbLog = new();
            sbLog.AppendFormat("Crash Happened at: {0}\n\n", DateTime.UtcNow);
            sbLog.AppendFormat("Current Config:\n{0}\n\n", JsonConvert.SerializeObject(Config.instance, Formatting.Indented));
            sbLog.AppendFormat("Exception:\n{0}\n\n", JsonConvert.SerializeObject(ex, Formatting.Indented));
            sbLog.AppendFormat("Domain:\n{0}\n\n", JsonConvert.SerializeObject(appDomain, Formatting.Indented));

            // Write to log file
            Logger.instance.T("Writing Crash Log to file");

            writer.Write(sbLog.ToString());
            writer.Flush();

            // Info
            Logger.instance.E("Unable to recover from error - Send crash log to COBOD International!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            Logger.instance.D("Cancel key pressed");
            this.SetCancellationToken();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnProcessExit(object sender, EventArgs args)
        {
            Logger.instance.D("Process Exit");
            this.SetCancellationToken();
        }

        private void SetCancellationToken() 
        { 
            Logger.instance.T("Setting Cancellation token");
            this.cancellationTokenSource.Cancel(); 
        }
    }
}