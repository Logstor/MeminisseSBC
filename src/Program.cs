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

            // Retrieve state
            MachineStatus status;
            while(running)
            {
                try 
                {
                    // Get machine status
                    status = await dataAccess.requestStatus();
                    logger.T(string.Format("Status {0}", status));

                    // If machine is processing, then start logging
                    if (status == MachineStatus.Starting || status == MachineStatus.Processing)
                    {
                        logger.T("Machine Starting or Processing");
                        LogController log = new LogController(logger);
                        await log.start();
                    }
                }
                catch(JsonException e)
                {
                    logger.E(string.Format("Deserialization failed! {0}", e.Message));
                }
                catch(Exception e)
                {
                    logger.E(string.Format("Other Exception!: {0}", e.Message));
                }

                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="model"></param>
        /// <exception cref="System.OperationCanceledException"/>
        /// <exception cref="SocketException"/>
        private static async Task UpdateModel(SubscribeConnection connection, ObjectModel model)
        {
            logger.T("Quering Object Model");
            using JsonDocument patch = await connection.GetObjectModelPatch(cancellationToken);
            logger.T("Object Model received successfully");

            logger.T("Trying to update Model from Json");
            model.UpdateFromJson(patch.RootElement);
            logger.T("Updated Model from Json Success");
        }

        /// <summary>
        /// Eventhandler for the logging endpoint.
        /// </summary>
        /// <param name="unixSocket"></param>
        /// <param name="requestConnection"></param>
        /// <returns></returns>
        private static async void Handler(HttpEndpointUnixSocket unixSocket, HttpEndpointConnection requestConnection)
        {
            // Log
            logger.T("Got new request");

            // Receive and request
            DuetAPI.Commands.ReceivedHttpRequest request = await requestConnection.ReadRequest();
            await requestConnection.SendResponse(200, "", HttpResponseType.PlainText);

            // Append everything to StringBuilder
            StringBuilder sb = new StringBuilder();
            sb.Append("Headers:\n");
            foreach((var header, var value) in request.Headers)
            {
                sb.Append(string.Format("Header: {0}, Value: {1}", header, value));
            }

            sb.Append("\nQueries\n");
            foreach((string query, string value) in request.Queries)
            {
                sb.Append(string.Format("Query: {0}, Value: {1}"));
            }

            // Write everything to console
            logger.I(sb.ToString());
        }
    }
}
