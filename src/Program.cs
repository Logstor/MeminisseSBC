using System;
using DuetAPIClient;
using DuetAPI;
using DuetAPI.Commands;
using DuetAPI.Connection;
using DuetAPI.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;

namespace Meminisse
{
    public static class Program
    {
        /// <summary>
        /// Logger instance which is used for logging through the whole application
        /// </summary>
        /// <returns></returns>
        static Logger logger;

        static Config config;

        /// <summary>
        /// 
        /// </summary>
        static bool running = true;

        static IDataAccess dataAccess;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static private CancellationTokenSource CancelSource = new();

        /// <summary>
        /// 
        /// </summary>
        static private CancellationToken cancellationToken = CancelSource.Token;

        public static int Main(string[] args)
        {
            // Deal with program termination requests (SIGTERM and Ctrl+C)
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                if (!CancelSource.IsCancellationRequested)
                {
                    CancelSource.Cancel();
                    running = false;
                }
            };
            Console.CancelKeyPress += (sender, e) =>
            {
                if (!CancelSource.IsCancellationRequested)
                {
                    e.Cancel = true;
                    CancelSource.Cancel();
                    running = false;
                }
            };

            // Initialize configuration
            config = Config.instance;

            // Initialize logger
            logger = new Logger(config.ConsoleLogLevel);

            // Start actual main
            MainTask(args).Wait();
            return 0;
        }

        public static async Task MainTask(string[] args)
        {
            logger.I("Meminisse started!");

            // Init DataAccess - Inject API here
            dataAccess = CodeDataAccess.getInstance(logger);

            // Start Log Controller
            while(running)
            {
                try 
                {
                    LogController logController = new LogController(logger);
                    await logController.start();
                }
                catch(JsonException e)
                {
                    logger.E(string.Format("Deserialization failed! {0}", e.Message));
                }
                catch(Exception e)
                {
                    logger.E(string.Format("Other Exception!: {0}", e.Message));
                }
            }
        }
    }
}
