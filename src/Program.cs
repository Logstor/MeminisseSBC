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

            // Start Log Controller
            while(!cancellationToken.IsCancellationRequested)
            {
                List<Task> taskList = new List<Task>(2);
                try 
                {
                    LogController logController = new LogController(cancellationToken);
                    taskList.Add(logController.start());

                    
                    Task.WhenAll(taskList);
                }
                catch(JsonException e)
                {
                    logger.E(string.Format("Deserialization failed! {0}", e.Message));
                }
            }
        }
    }
}
