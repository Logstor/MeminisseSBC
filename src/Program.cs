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

        static void CTRLC(object sender, ConsoleCancelEventArgs eventArgs)
        {
            logger.I("Received SIGINT");
            running = false;
        }

        public static int Main(string[] args)
        {
            // Signal handler
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CTRLC);

            // Start actual main
            MainTask(args).Wait();
            return 0;
        }
        public static async Task MainTask(string[] args)
        {
            logger.I("Hello COBOD");

            CommandConnection conn = null;
            HttpEndpointUnixSocket socket;

            try {
                // Create HTTPEndpoint
                (socket, conn) = await CreateEndpoint();
                logger.I("HTTPEndpoint created!");

                // Add eventhandler
                socket.OnEndpointRequestReceived += Handler;

                while(running);
            }
            catch (SocketException e) {
                logger.E(string.Format("Error creating Custom HTTPEndpoint\nException: {0}", e.ToString()));
            }
            finally {
                logger.I("Removing Endpoint");
                if (conn != null && conn.IsConnected)
                    await conn.RemoveHttpEndpoint(endpointType, ns, path);
            }
        }

        private static async Task<(HttpEndpointUnixSocket, CommandConnection)> CreateEndpoint()
        {
            // Use Command Connection
            CommandConnection cmdConn = new CommandConnection();
            await cmdConn.Connect(Defaults.FullSocketPath);

            // Add the Endpoint
            return (await cmdConn.AddHttpEndpoint(endpointType, ns, path), cmdConn);
        }

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
