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

namespace Meminisse 
{
    /// <summary>
    /// 
    /// </summary>
    public class CodeDataAccess : IDataAccess
    {
        private static CodeDataAccess instance;

        private CommandConnection commandConnection;

        private Code codeM408S4;

        private int maxRetry = 3;

        private Logger logger;

        private CodeDataAccess(Logger logger) 
        {
            commandConnection = new CommandConnection();
            this.logger = logger;

            this.codeM408S4 = new Code("M408 S4");
            this.codeM408S4.Channel = CodeChannel.SBC;
            this.codeM408S4.Flags = CodeFlags.IsPrioritized;
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

        async Task<EntityWrap> IDataAccess.requestFull()
        {
            await this.CheckConnection();

            // Retrieve data
            int tries = 0;
            while(true)
            {
                try
                {
                    // Get data
                    string json = await this.commandConnection.GetSerializedObjectModel();

                    // Parse data
                    JObject obj = JObject.Parse(json);

                    (Position position, Babystep babystep) = this.ParsePositionAndBabystep(obj);
                    return new EntityWrap.Builder()
                                .Position(position)
                                .Speed(this.ParseSpeed(obj))
                                .Layer(this.ParseLayer(obj))
                                .Time(this.ParseTime(obj))
                                .Extrusion(this.ParseExtrusion(obj))
                                .Babystep(babystep)
                                .Build();
                }
                catch (Exception)
                {
                    if (++tries == this.maxRetry)
                        throw;
                }
            }
        }

        /// <summary>
        /// Retrieves the position of the Machine.
        /// </summary>
        /// <returns>PositionEntity</returns>
        async Task<Position> IDataAccess.requestPosition()
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
                    List<float> pos = obj["coords"]["xyz"].ToObject<List<float>>();
                    MachineStatus status = this.ParseMachineStatus(obj);

                    return new Position(pos);
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

        async Task<string> IDataAccess.requestCurrentFilePath()
        {
            await this.CheckConnection();

            for (int i=0; i < maxRetry; i++)
            {
                try
                {
                    // Query
                    string json = await this.commandConnection.GetSerializedObjectModel();

                    // Parse
                    JObject obj = JObject.Parse(json);
                    return obj["job"]["file"]["fileName"].ToObject<string>();
                }
                catch (Exception)
                {
                    this.logger.D("Failed retrieving filename, Retrying ...");
                }
            }

            // This is reached on error
            this.logger.E("Couldn't retrieve filename!");
            return "Error";
        }

        /// <summary>
        /// Performs the code "M408 S4" and returns the result.
        /// </summary>
        /// <returns>string</returns>
        /// <exception cref="Exception">Throws with appropriate message</Exception>
        private async Task<string> M408S4()
        {
            CodeResult res = await this.commandConnection.PerformCode(this.codeM408S4);

            // Check if things went okay
            if (!res.IsSuccessful) throw new Exception("CodeResult was unsuccessful");
            else if (res.IsEmpty) throw new Exception("CodeResult was empty");

            return res.ToString();
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

        private (Position, Babystep) ParsePositionAndBabystep(JObject obj)
        {
            // Initial Position
            Position pos = new Position(new List<float>{.0f, .0f, .0f, .0f});
            Babystep baby = new Babystep(new List<float>{.0f, .0f, .0f, .0f});

            // JTokens
            JToken axes;
            JToken curr;

            // Get Axis
            axes = obj.SelectToken("move.axes");
            if ( axes != null )
            {
                int currAxis = 0;
                float currPos = 0;
                float currBaby = 0;
                string currLetter;
                do 
                {
                    // The use of [] out of index throws
                    try { curr = axes[currAxis++]; }
                    catch (Exception) { break; }

                    // Break if we don't find anymore axis
                    if (curr == null)
                        break;

                    currPos = curr.SelectToken("machinePosition").ToObject<float>();
                    currBaby = curr.SelectToken("babystep").ToObject<float>();
                    currLetter = curr.SelectToken("letter").ToObject<string>();

                    switch(currLetter)
                    {
                        case "X":
                            pos.x = currPos;
                            baby.xBaby = currBaby;
                            break;
                        case "Y":
                            pos.y = currPos;
                            baby.yBaby = currBaby;
                            break;
                        case "Z":
                            pos.z = currPos;
                            baby.zBaby = currBaby;
                            break;
                        case "U":
                            pos.u = currPos;
                            baby.uBaby = currBaby;
                            break;
                        default:
                            throw new Exception("Couldn´t parse axis");
                    }
                } while(true);
            }

            return (pos, baby);
        }

        private Speed ParseSpeed(JObject obj)
        {
            Speed speed = new Speed(0, 0, 0);

            // Parse
            JToken currentMove = obj.SelectToken("move.currentMove");
            if ( currentMove != null )
            {
                speed.speedFactor = obj["move"]["speedFactor"].ToObject<float>();
                speed.speedRequested = currentMove.SelectToken("requestedSpeed").ToObject<int>();
                speed.speedTop = currentMove.SelectToken("topSpeed").ToObject<int>();
            }

            return speed;
        }

        private Layer ParseLayer(JObject obj)
        {
            Layer layer = new Layer(0, 0);

            // Parse
            layer.currLayer = obj["job"]["layer"].ToObject<int>();
            layer.layerTimeSec = obj["job"]["layerTime"].ToObject<float>();

            return layer;
        }

        private Time ParseTime(JObject obj)
        {
            Time time = new Time(0, 0);

            // Parse
            time.printDurationSec = obj["job"]["duration"].ToObject<int>();
            time.pauseDurationSec = obj["job"]["pauseDuration"].ToObject<int>();

            return time;
        }

        private Extrusion ParseExtrusion(JObject obj)
        {
            Extrusion extrusion = new Extrusion();

            // Parse
            extrusion.ExtrusionFactor = obj["move"]["extruders"][0]["factor"].ToObject<float>();

            return extrusion;
        }

        private async Task CheckConnection()
        {
            if (!this.commandConnection.IsConnected)
                await this.commandConnection.Connect();
        }
    }
}

