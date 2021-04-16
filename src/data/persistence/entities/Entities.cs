using System;
using System.Collections.Generic;

namespace Meminisse
{
    public interface ILogEntity
    {
        LogEntity GetEntityType();
    }

    public enum LogEntity
    {
        Position,
        Layer,
        Time,
        Speed,
        Extrusion,
        Babystep

    }

    public class Position : ILogEntity
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int u { get; set; }

        public Position() {}

        public Position(int x, int y, int z, int u)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.u = u;
        }

        public Position(List<int> pos)
        {
            switch(pos.Count)
            {
                case 4:
                    this.y = pos[3];
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

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Speed; }
    }

    public class Layer : ILogEntity
    {
        public int currLayer { get; set; }
        public int layerTime { get; set; }

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Layer; }
    }

    public class Time : ILogEntity
    {
        public int printDuration { get; set; }

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Time; }
    }

    public class Extrusion : ILogEntity
    {
        public int ExtrusionFactor { get; set; }

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Extrusion; }
    }

    public class Babystep : ILogEntity
    {
        public int babystep { get; set; }

        LogEntity ILogEntity.GetEntityType() { return LogEntity.Babystep; }
    }
}