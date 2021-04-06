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
    /// <summary>
    /// Wrapper for an Endpoint and its ID.
    /// </summary>
    public struct EndpointWrapper
    {
        public int id { get; private set; }
        public HttpEndpointUnixSocket socket { get; private set; }

        public EndpointWrapper(int id, HttpEndpointUnixSocket socket)
        {
            this.id = id;
            this.socket = socket;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EndpointController
    {
        /// <summary>
        /// Indicates whether the controller instance has been initialized.
        /// </summary>
        /// <value></value>
        public bool isInitialized { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private CommandConnection connection;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<int, HttpEndpointUnixSocket> endpoints;

        /// <summary>
        /// 
        /// </summary>
        private int currKey = 0;

        /// <summary>
        /// 
        /// </summary>
        private string socketPath;

        private CancellationToken cancellationToken;

        public EndpointController(string socketPath = Defaults.FullSocketPath, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.connection = new();
            this.socketPath = socketPath;
            this.endpoints = new();
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Makes the controller connect to the Controlserver, which is required before any operation can be made.
        /// </summary>
        /// 
        /// <returns>void</returns>
        /// <exception cref="DuetAPI.Connection.IncompatibleVersionException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="System.OperationCanceledException"></exception>
        /// <exception cref="System.Net.Sockets.SocketException"></exception>
        public async void Init()
        {
            // Make sure not to initialize again.
            if (this.isInitialized) return;

            await this.connection.Connect(this.socketPath, cancellationToken);
            this.isInitialized = true;
        }

        /// <summary>
        /// Cleaning up all the remaining HttpEndpoints and Connections.
        /// </summary>
        public void CleanUp()
        {
            if (this.isInitialized && this.connection.IsConnected)
            {
                List<Task> tasks = new();

                // Run over every Endpoint and remove it
                foreach ((int key, HttpEndpointUnixSocket endpoint) in this.endpoints)
                    tasks.Add(this.connection.RemoveHttpEndpoint(endpoint.EndpointType, endpoint.Namespace, endpoint.EndpointPath));

                // Busy wait
                foreach (Task task in tasks)
                    task.Wait();

                // Close the connection
                this.connection.Close();
            }
        }

        /// <summary>
        /// Creates an Endpoint and return the key and object itself to be used.
        /// </summary>
        /// <param name="ns">Namespace of the Endpoint</param>
        /// <param name="path">Path of the Endpoint</param>
        /// <returns>(key, HttpEndpointUnixSocket)</returns>
        /// <exception>ArgumentException</exception>
        /// <exception>InvalidOperationException</exception>
        /// <exception>IOException</exception>
        /// <exception>OperationCanceledException</exception>
        /// <exception>SocketException</exception>
        public async Task<EndpointWrapper> CreateEndpoint(string ns, string path)
        {
            // Add the endpoint
            HttpEndpointUnixSocket socket = await this.connection.AddHttpEndpoint(HttpEndpointType.GET, ns, path, false, backlog: 4, cancellationToken);

            // Add to the Dictionary
            int key = this.currKey++;
            this.endpoints.Add(key, socket);

            return new EndpointWrapper(key, socket);
        }

        public async void DeleteEndpoint(int key)
        {
            // Retrieve from dictionary
            HttpEndpointUnixSocket socket;
            bool success = this.endpoints.TryGetValue(key, out socket);

            // If it exists remove the connection
            if (success)
                await this.connection.RemoveHttpEndpoint(socket.EndpointType, socket.Namespace, socket.EndpointPath, cancellationToken);
        }
    }
}