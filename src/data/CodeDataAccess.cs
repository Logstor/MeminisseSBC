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

    private CodeDataAccess() 
    {
        commandConnection = new CommandConnection();
    }

    public static CodeDataAccess getInstance()
    {
        if (instance == null)
            instance = new();
        return instance;
    }

    async Task<PositionEntity> IDataAccess.requestPosition()
    {
        await this.checkConnection();

        // Retrieve data
        string json = await this.commandConnection.PerformSimpleCode("M408 S4", CodeChannel.SBC);

        // Get Position
        JObject obj = JObject.Parse(json);
        List<int> pos = obj["coords"]["xyz"].ToObject<List<int>>();

        return new PositionEntity(pos.ToArray());
    }

    async Task<MachineStatus> IDataAccess.requestStatus()
    {
        await this.checkConnection();

        // Retrieve data
        string json = await this.commandConnection.PerformSimpleCode("M408", CodeChannel.SBC);

        // Get Status
        JObject obj = JObject.Parse(json);

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

    private async Task checkConnection()
    {
        if (!this.commandConnection.IsConnected)
            await this.commandConnection.Connect();
    }
}