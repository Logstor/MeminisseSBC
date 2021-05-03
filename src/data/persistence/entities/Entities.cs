using System;
using System.Collections.Generic;

using DuetAPI.ObjectModel;

namespace Meminisse
{
    public interface ILogEntity
    {
        LogEntity GetEntityType();
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

    public class Position : ILogEntity
    {
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

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Position; }
    }

    public class Speed : ILogEntity
    {
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

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Speed; }
    }
    
    public class Time : ILogEntity
    {
        public int printDurationSec { get; set; }
        public int pauseDurationSec { get; set; }

        public Time() {}

        public Time(int duration, int pauseDuration)
        {
            this.printDurationSec = duration;
            this.pauseDurationSec = pauseDuration;
        }

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Time; }
    }

    public class Extrusion : ILogEntity
    {
        public float ExtrusionFactor { get; set; }

        public Extrusion() {}

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Extrusion; }
    }

    public class Babystep : ILogEntity
    {
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

        public Babystep(List<float> baby)
        {
            switch(baby.Count)
            {
                case 4:
                    this.uBaby = baby[3];
                    goto case 3;
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

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Babystep; }
    }

    public class Voltage : ILogEntity
    {
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

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Voltage; }
    }
}