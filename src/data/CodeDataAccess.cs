using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DuetAPI;
using DuetAPIClient;
using DuetAPI.Commands;
using DuetAPI.ObjectModel;

public class CodeDataAccess : IDataAccess
{
    private static CodeDataAccess instance;

    private CommandConnection commandConnection;

    private int maxRetry = 3;

    private Logger logger;

    private CodeDataAccess(Logger logger) 
    {
        commandConnection = new CommandConnection();
        this.logger = logger;
    }

    /// <summary>
    /// Gets the instance of the CodeDataAccess singleton. If it's the first time calling 
    /// this method, then it uses the logger parameter to set the logger.
    /// </summary>
    /// <param name="logger">To be set if it's the first time calling this method</param>
    /// <returns>CodeDataAccess instance</returns>
    public static CodeDataAccess getInstance(Logger logger)
    {
        if (instance == null)
            instance = new(logger);
        return instance;
    }

    /// <summary>
    /// Retrieves the position of the Machine.
    /// </summary>
    /// <returns>PositionEntity</returns>
    async Task<PositionEntity> IDataAccess.requestPosition()
    {
        await this.CheckConnection();

        // Retrieve data
        Exception ex = null;
        for (int i=0; i < this.maxRetry; i++)
        {
            try
            {
                string json = await this.M408S4();

                // Get Position
                JObject obj = JObject.Parse(json);
                List<int> pos = obj["coords"]["xyz"].ToObject<List<int>>();
                MachineStatus status = this.ParseMachineStatus(obj);

                return new PositionEntity(pos.ToArray(), status);
            }
            catch (Exception e)
            {
                ex = e;
                this.logger.D("Failed retrieving position ... Retrying");
            }
        }
        
        // Error here
        throw new Exception(string.Format("Failed retrieving positions - Exception: {0}", ex.Message));
    }

    async Task<MachineStatus> IDataAccess.requestStatus()
    {
        await this.CheckConnection();

        // Retrieve data
        for (int i=0; i < maxRetry; i++)
        {
            try 
            {
                string json = await this.M408S4();

                // Get Status
                JObject obj = JObject.Parse(json);
                return this.ParseMachineStatus(obj);
            }
            catch (Exception)
            {
                this.logger.D("Failed retrieving status, Retrying ...");
            }
        }

        // This indicates error
        this.logger.E("Couldn't retrieve MachineStatus");
        return MachineStatus.Off;
    }

    private async Task<string> M408S4()
    {
        return await this.commandConnection.PerformSimpleCode("M408 S4", CodeChannel.SBC);
    }

    /// <summary>
    /// Getting the MachineStatus from a M408 S4 json parsed to a JObject.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>MachineStatus</returns>
    private MachineStatus ParseMachineStatus(JObject obj)
    {
        switch(obj["status"].ToObject<string>())
        {
            case "I":
                return MachineStatus.Idle;
            case "A":
                return MachineStatus.Paused;
            case "P":
                return MachineStatus.Processing;
            default:
                return MachineStatus.Paused;
        }
    }

    private async Task CheckConnection()
    {
        if (!this.commandConnection.IsConnected)
            await this.commandConnection.Connect();
    }
}