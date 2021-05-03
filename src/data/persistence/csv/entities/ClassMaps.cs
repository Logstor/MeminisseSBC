using System.Globalization;

using CsvHelper.Configuration;

namespace Meminisse
{
    public sealed class PositionMap : ClassMap<Position>
    {
        public PositionMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.x).TypeConverter<DecConvert>();
            Map(m => m.y).TypeConverter<DecConvert>();
            Map(m => m.z).TypeConverter<DecConvert>();
            Map(m => m.u).TypeConverter<DecConvert>();
        }
    }

    public sealed class SpeedMap : ClassMap<Speed>
    {
        public SpeedMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }

    public sealed class TimeMap : ClassMap<Time>
    {
        public TimeMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }

    public sealed class ExtrusionMap : ClassMap<Extrusion>
    {
        public ExtrusionMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.ExtrusionFactor).TypeConverter<DecConvert>();
        }
    }

    public sealed class BabystepMap : ClassMap<Babystep>
    {
        public BabystepMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.zBaby).TypeConverter<DecConvert>();
            Map(m => m.xBaby).Ignore();
            Map(m => m.yBaby).Ignore();
            Map(m => m.uBaby).Ignore();
        }
    }

    public sealed class VoltageMap : ClassMap<Voltage>
    {
        public VoltageMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.v12Curr).TypeConverter<DecConvert>();
            Map(m => m.v12Min).TypeConverter<DecConvert>();
            Map(m => m.v12Max).TypeConverter<DecConvert>();
            Map(m => m.vInCurr).TypeConverter<DecConvert>();
            Map(m => m.vInMin).TypeConverter<DecConvert>();
            Map(m => m.vInMax).TypeConverter<DecConvert>();
        }
    }
}
