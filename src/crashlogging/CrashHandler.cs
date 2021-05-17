using System;
using System.IO;
using System.Threading;

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
            this.SetCancellationToken();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            this.SetCancellationToken();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnProcessExit(object sender, EventArgs args)
        {
            this.SetCancellationToken();
        }

        private void SetCancellationToken() 
        { 
            Logger.instance.T("Setting Cancellation token");
            this.cancellationTokenSource.Cancel(); 
        }
    }
}