using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

using DuetAPI.ObjectModel;
using DuetAPI.Connection;
using DuetAPIClient;

namespace Meminisse 
{
    /// <summary>
    /// Provides access to the data on the duet.
    /// 
    /// This class is thread safe.
    /// </summary>
    public class ConnDataAccess : IDataAccess, IDisposable
    {
        private static ConnDataAccess instance;

        private CancellationToken cancellationToken;

        private Logger logger;

        private SemaphoreSlim gate = new SemaphoreSlim(1);

        private SubscribeConnection subConn;

        /// <summary>
        /// Asynchronous task that continuously updates the ObjectModel.
        /// </summary>
        private Task retrieverTask;

        private ObjectModel model;


        /// <summary>
        /// Getting the instance. If it's already initialized it ignores the Logger and CancellationToken.
        /// 
        /// The token is used to stop the asynchronous updates of the ObjectModel.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>ConnDataAccess instance</returns>
        public static ConnDataAccess getInstance(Logger logger, CancellationToken cancellationToken = default)
        {
            if (instance == null)
                instance = new(logger, cancellationToken);
            return instance;
        }

        private ConnDataAccess(Logger logger, CancellationToken cancellationToken)
        {
            this.logger = logger;
            this.cancellationToken = cancellationToken;

            // Create sub connection
            this.logger.T("Creating and Connecting SubscribeConnection");
            this.CreateAndInitSubConnection();

            // Get full ObjectModel
            this.logger.T("Retrieving Initial ObjectModel");
            Task<ObjectModel> objTask = this.subConn.GetObjectModel(this.cancellationToken);
            objTask.Wait();
            this.model = objTask.Result;

            // Start retrieving object model
            this.logger.T("Starting Retriever task");
            this.retrieverTask = Task.Factory.StartNew(
                this.UpdateAction, 
                this.cancellationToken, 
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Default);
        }

        void IDisposable.Dispose()
        {
            // Stop Retriever thread
            if (this.retrieverTask != null)
                this.retrieverTask.Dispose();

            // Close connection
            if (this.subConn != null)
                this.subConn.Dispose();

            // Release Semaphore
            if (this.gate != null)
                this.gate.Dispose();
        }

        /// <summary>
        /// Retrieves a full entity wrap.
        /// </summary>
        /// <returns>EntityWrap</returns>
        async Task<EntityWrap> IDataAccess.requestFull()
        {
            // Needed to put into Task to fulfill interface
            return await Task.Run<EntityWrap>(() => {
                // Acquire lock
                this.gate.Wait(this.cancellationToken);

                EntityWrap entityWrap = new EntityWrap.Builder()
                    .Babystep(new Babystep().ParseFromModel(this.model))
                    .Extrusion(new Extrusion().ParseFromModel(this.model))
                    .Position(new Position().ParseFromModel(this.model))
                    .Speed(new Speed().ParseFromModel(this.model))
                    .Time(new Time().ParseFromModel(this.model))
                    .Voltage(new Voltage().ParseFromModel(this.model))
                    .Build(this.model.State.Status);

                // Release lock before returning object
                this.gate.Release();
                return entityWrap;
            });
        }

        async Task<MachineStatus> IDataAccess.requestStatus()
        {
            return await Task.Run<MachineStatus>(() => { 
                this.gate.Wait(this.cancellationToken);
                MachineStatus status = this.model.State.Status; 
                this.gate.Release();
                return status;
            } );
        }
        async Task<Position> IDataAccess.requestPosition()
        {
            return await Task.Run<Position>(() => {
                this.gate.Wait(this.cancellationToken);
                Position pos = new Position().ParseFromModel(this.model);
                this.gate.Release();
                return pos;
            });
        }

        async Task<string> IDataAccess.requestCurrentFilePath()
        {
            return await Task.Run<string>(() => {
                this.gate.Wait(this.cancellationToken);
                string filePath = this.model.Job.File.FileName;
                this.gate.Release();
                return filePath;
            });
        }

        /// <summary>
        /// Create and Connect a new SubscribeConnection.
        /// </summary>
        /// <returns>SubscribeConnection</returns>
        private void CreateAndInitSubConnection()
        {
            this.subConn = new();
            this.subConn.Connect(mode: SubscriptionMode.Patch, filters: null, cancellationToken: this.cancellationToken).Wait();
        }

        /// <summary>
        /// Updates this instance's ObjectModel continuously.
        /// </summary>
        private void UpdateAction()
        {
            while(!this.cancellationToken.IsCancellationRequested)
            {
                // Check connection
                this.logger.T("Checking connection");
                if (!this.CheckConnection())
                {
                    // Update connection
                    this.logger.D("Connection Dead - Creating new");
                    this.CreateAndInitSubConnection();
                }

                // Update ObjectModel
                JsonDocument json;
                try 
                {
                    // Get ObjectModel Patch
                    this.logger.T("Retrieving ObjectModel Patch");
                    Task<JsonDocument> task = this.subConn.GetObjectModelPatch(this.cancellationToken);
                    task.Wait();
                    json = task.Result;
                }
                catch (Exception e)
                {
                    this.logger.W($"Retrieving ObjectModel failed: {e.Message}");
                    continue;
                }

                // Update the ObjectModel
                this.logger.T("Updating ObjectModel from Patch");
                this.gate.Wait(this.cancellationToken);
                this.model.UpdateFromJson(json.RootElement);
                this.gate.Release();
            }
        }
    
        /// <summary>
        /// Checks the current Subscribe Connection if it's still alive and working.
        /// </summary>
        /// <returns>False if dead</returns>
        private Boolean CheckConnection()
        {
            // Send null-byte to ensure connection is still alive.
            try { this.subConn.Poll(); }
            catch(Exception) { return false; }

            // Success
            return true;
        }
    }
}