using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Meminisse 
{
    /// <summary>
    /// Conversion to string which goes into CSV.
    /// Makes sure that the given number has the specified amount of decimals.
    /// </summary>
    public class DecConvert : DecimalConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return ( (float)value ).ToString("f2");
        }
    }
}