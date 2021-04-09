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
using System.Buffers;
using System.IO;
using System.Collections.Generic;

namespace Meminisse
{
    public static class Program
    {
        /// <summary>
        /// Logger instance which is used for logging through the whole application
        /// </summary>
        /// <returns></returns>
        static Logger logger = new Logger();

        /// <summary>
        /// 
        /// </summary>
        static bool running = true;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static private CancellationTokenSource CancelSource = new();

        /// <summary>
        /// 
        /// </summary>
        static private CancellationToken cancellationToken = CancelSource.Token;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static private EndpointController epHandle = new EndpointController(cancellationToken: cancellationToken);

        /// <summary>
        ///  HTTP Endpoint Namespace
        /// </summary>
        const string ns = "COBOD";

        /// <summary>
        ///  HTTP Endpoint Path
        /// </summary>
        const string path = "Log";

        /// <summary>
        /// Type of the HTTPEndpoint
        /// </summary>
        const HttpEndpointType endpointType = HttpEndpointType.GET;

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

            // Start actual main
            MainTask(args).Wait();
            return 0;
        }

        public static async Task MainTask(string[] args)
        {
            logger.I("Meminisse started!");

            // Init EndpointController
            epHandle.Init();

            // Create endpoint
            EndpointWrapper endpoint;
            try {
                // Create HTTPEndpoint
                endpoint = await epHandle.CreateEndpoint(ns, path);

                // Add eventhandler
                endpoint.socket.OnEndpointRequestReceived += Handler;

                logger.D("Endpoint created!");
            }
            catch (SocketException e) {
                logger.E(string.Format("Error creating Custom HTTPEndpoint\nException: {0}", e.ToString()));
            }

            // Retrieve state
            logger.T("Start Subscribe connection");
            using SubscribeConnection subConn = new();

        #pragma warning disable CS0612
            await subConn.Connect(mode: SubscriptionMode.Patch, filters: null, cancellationToken: cancellationToken);
        #pragma warning restore CS0612

            logger.D("Connection established");

            ObjectModel currModel = new();
            while(running)
            {
                try 
                {
                    // Update the ObjectModel
                    await UpdateModel(subConn, currModel);

                    // If machine is processing, then start logging
                    if (currModel.State.Status == MachineStatus.Processing)
                    {
                        
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
            using JsonDocument patch = await connection.GetObjectModelPatch(cancellationToken);
            model.UpdateFromJson(patch.RootElement);
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
