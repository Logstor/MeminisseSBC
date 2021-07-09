using System;
using System.Collections.Generic;

using DuetAPI.ObjectModel;

namespace Meminisse
{
    public interface ILogEntity<T>
    {
        LogEntity GetEntityType();

        /// <summary>
        /// Takes in the Duet ObjectModel and returns T.
        /// </summary>
        /// <param name="model">ObjectModel</param>
        T ParseFromModel(ObjectModel model);
    }

    public enum LogEntity
    {
        Status,
        Position,
        Layer,
        Time,
        Speed,
        Extrusion,
        Babystep,
        Voltage
    }

    /// <summary>
    /// This wraps all loggable Entities.
    /// 
    /// Create with EntityWrap.Builder
    /// </summary>
    public class EntityWrap
    {
        public MachineStatus machineStatus { get; private set; }
        public Position position { get; private set; }
        public Speed speed { get; private set; }
        public Time time { get; private set; }
        public Extrusion extrusion { get; private set; }
        public Babystep babystep { get; private set; }
        public Voltage voltage { get; private set; }

        private EntityWrap(MachineStatus status, Position position, Speed speed, Time time, Extrusion extrusion, Babystep babystep, Voltage voltage)
        {
            this.machineStatus = status;
            this.position = position;
            this.speed = speed;
            this.time = time;
            this.extrusion = extrusion;
            this.babystep = babystep;
            this.voltage = voltage;
        }

        public class Builder
        {
            private Position position;
            private Speed speed;
            private Time time;
            private Extrusion extrusion;
            private Babystep babystep;
            private Voltage voltage;

            public EntityWrap Build(MachineStatus machineStatus)
            {
                if (position == null || speed == null || time == null || extrusion == null || babystep == null || voltage == null)
                    throw new ArgumentException("Atleast one parameter needs to be not null!");

                return new EntityWrap(machineStatus, this.position, this.speed, this.time, extrusion, this.babystep, this.voltage);
            }
            
            public Builder Position(Position position) { this.position = position; return this; }
            public Builder Speed(Speed speed) { this.speed = speed; return this; }
            public Builder Time(Time time) { this.time = time; return this; }
            public Builder Extrusion(Extrusion extrusion) { this.extrusion = extrusion; return this; }
            public Builder Babystep(Babystep babystep) { this.babystep = babystep; return this; }
            public Builder Voltage(Voltage voltage) { this.voltage = voltage; return this; }
        }
    }

    public class Position : ILogEntity<Position>
    {
        public const string id = "0";
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float u { get; set; }

        public Position() {}

        public Position(float x, float y, float z, float u)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.u = u;
        }

        public override string ToString()
        {
            return id;
        }

        public Position(List<float> pos)
        {
            switch(pos.Count)
            {
                case 4:
                    this.u = pos[3];
                    goto case 3;
                case 3:
                    this.z = pos[2];
                    this.y = pos[1];
                    this.x = pos[0];
                    break;
                default:
                    throw new Exception(string.Format("Creating Position, pos argument needs atleast 3 or max 4 ints: {0}", 
                        pos.Count));
            }
        }

        LogEntity ILogEntity<Position>.GetEntityType() { return LogEntity.Position; }

        public Position ParseFromModel(ObjectModel model)
        {
            // Retrieve axis
            ModelCollection<Axis> axisCollection = model.Move.Axes;

            // Update properties
            foreach (Axis axis in axisCollection)
            {
                this.MapAxis(axis);
            }

            return this;
        }

        /// <summary>
        /// Takes an Axis object, and updates corressponding parameter.
        /// </summary>
        /// <param name="axis">Axis</param>
        private void MapAxis(Axis axis)
        {
            switch(axis.Letter)
            {
                case 'X':
                    this.x = axis.MachinePosition ?? -1.0f;
                    break;

                case 'Y':
                    this.y = axis.MachinePosition ?? -1.0f;
                    break;

                case 'Z':
                    this.z = axis.MachinePosition ?? -1.0f;
                    break;

                case 'U':
                    this.u = axis.MachinePosition ?? -1.0f;
                    break;

                default:
                    throw new ArgumentException($"Invalid axis letter: ${axis.Letter}");
            }
        }
    }

    public class Speed : ILogEntity<Speed>
    {
        public const string id = "1";
        public int speedRequested { get; set; }
        public int speedTop { get; set; }
        public float speedFactor { get; set; }

        public Speed() {}

        public Speed(int requested, int top, float speedFactor)
        {
            this.speedRequested = requested;
            this.speedTop = top;
            this.speedFactor = speedFactor;
        }

        public override string ToString()
        {
            return id;
        }

        LogEntity ILogEntity<Speed>.GetEntityType() { return LogEntity.Speed; }

        public Speed ParseFromModel(ObjectModel model)
        {
            this.speedFactor = model.Move.SpeedFactor;
            this.speedRequested = ((int)model.Move.CurrentMove.RequestedSpeed);
            this.speedTop = ((int)model.Move.CurrentMove.TopSpeed);

            return this;
        }
    }
    
    public class Time : ILogEntity<Time>
    {
        public const string id = "2";
        public int printDurationSec { get; set; }
        public int pauseDurationSec { get; set; }

        public Time() {}

        public Time(int duration, int pauseDuration)
        {
            this.printDurationSec = duration;
            this.pauseDurationSec = pauseDuration;
        }

        public override string ToString()
        {
            return id;
        }

        LogEntity ILogEntity<Time>.GetEntityType() { return LogEntity.Time; }

        public Time ParseFromModel(ObjectModel model)
        {
            this.pauseDurationSec = model.Job.PauseDuration ?? -1;
            this.printDurationSec = model.Job.Duration ?? -1;

            return this;
        }
    }

    public class Extrusion : ILogEntity<Extrusion>
    {
        public const string id = "3";
        public float ExtrusionFactor { get; set; }

        public Extrusion() {}

        public override string ToString()
        {
            return id;
        }

        LogEntity ILogEntity<Extrusion>.GetEntityType() { return LogEntity.Extrusion; }

        public Extrusion ParseFromModel(ObjectModel model)
        {
            this.ExtrusionFactor = model.Move.Extruders[0].Factor;

            return this;
        }
    }

    public class Babystep : ILogEntity<Babystep>
    {
        public const string id = "4";
        public float xBaby { get; set; }
        public float yBaby { get; set; }
        public float zBaby { get; set; }
        public float uBaby { get; set; }

        public Babystep() {}

        public Babystep(float x, float y, float z, float u)
        {
            this.xBaby = x;
            this.yBaby = y;
            this.zBaby = z;
            this.uBaby = u;
        }

        public override string ToString()
        {
            return id;
        }

        public Babystep(List<float> baby)
        {
            switch(baby.Count)
            {
                case 4:
                    this.uBaby = baby[3];
                    goto case 3;    // For some reason dotnet doesn't accept fallthrough.
                case 3:
                    this.zBaby = baby[2];
                    this.yBaby = baby[1];
                    this.xBaby = baby[0];
                    break;
                default:
                    throw new Exception(string.Format("Creating Babystep, baby argument needs atleast 3 or max 4 ints: {0}", 
                        baby.Count));
            }
        }

        LogEntity ILogEntity<Babystep>.GetEntityType() { return LogEntity.Babystep; }

        public Babystep ParseFromModel(ObjectModel model)
        {
            // Retrieve Model collection of Axis
            ModelCollection<Axis> axisCollection = model.Move.Axes;

            // Update properties for all axis
            foreach (Axis axis in axisCollection)
            {
                this.MapAxis(axis);
            }

            return this;
        }

        /// <summary>
        /// Takes an Axis object, and updates corressponding parameter.
        /// </summary>
        /// <param name="axis">Axis</param>
        private void MapAxis(Axis axis)
        {
            switch(axis.Letter)
            {
                case 'X':
                    this.xBaby = axis.Babystep;
                    break;

                case 'Y':
                    this.yBaby = axis.Babystep;
                    break;

                case 'Z':
                    this.zBaby = axis.Babystep;
                    break;

                case 'U':
                    this.uBaby = axis.Babystep;
                    break;

                default:
                    throw new ArgumentException($"Invalid axis letter: ${axis.Letter}");
            }
        }
    }

    public class Voltage : ILogEntity<Voltage>
    {
        public const string id = "5";
        public float v12Curr { get; set; }
        public float v12Min { get; set; }
        public float v12Max { get; set; }
        public float vInCurr { get; set; }
        public float vInMin { get; set; }
        public float vInMax { get; set; }

        public Voltage() {}

        public Voltage(float v12Curr, float v12Min, float v12Max, float vInCurr, float vInMin, float vInMax)
        {
            this.v12Curr = v12Curr;
            this.v12Min = v12Min;
            this.v12Max = v12Max;
            this.vInCurr = vInCurr;
            this.vInMin = vInMin;
            this.vInMax = vInMax;
        }

        public override string ToString()
        {
            return id;
        }

        LogEntity ILogEntity<Voltage>.GetEntityType() { return LogEntity.Voltage; }

        public Voltage ParseFromModel(ObjectModel model)
        {
            Board board = model.Boards[0];

            this.v12Curr    = board.V12.Current;
            this.v12Max     = board.V12.Max;
            this.v12Max     = board.V12.Min;

            this.vInCurr    = board.VIn.Current;
            this.vInMax     = board.VIn.Max;
            this.vInMin     = board.VIn.Min;

            return this;
        }
    }
}