using DuetAPIClient;
using DuetAPI;
using DuetAPI.Commands;
using DuetAPI.Connection;
using DuetAPI.ObjectModel;

using System;
using System.Collections.Generic;
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
            // Deal with program termination expected and unexpected
            CrashHandler crashHandler = new CrashHandler(CancelSource);

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
            dataAccess = ConnDataAccess.getInstance(logger, cancellationToken);

            // CSV Log Controller
            List<Task> tasks = new List<Task>(2);
            if (config.CSVLogging)
            {
                tasks.Add(new LogController(dataAccess, cancellationToken).start());
            }

            // OPC Server
            if (config.OPCServer)
            {
                logger.I("Starting Meminisse OPC Server");
                tasks.Add(new OPCApp(dataAccess, cancellationToken).Start());
            }

            // Wait
            await Task.WhenAll(tasks);
        }
    }
}
