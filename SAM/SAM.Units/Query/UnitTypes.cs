using System.Collections.Generic;
using System.Linq;

namespace SAM.Units
{
    public static partial class Query
    {
        public static List<UnitType> UnitTypes(this UnitCategory unitCategory, params UnitStyle[] unitStyles)
        {
            List<UnitType> result = new List<UnitType>();
            switch (unitCategory)
            {
                case UnitCategory.Temperature:

                    if(unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(UnitType.Celsius);
                        result.Add(UnitType.Fahrenheit);
                    }
                    else if(unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(UnitType.Celsius);
                    }
                    else if(unitStyles.Contains(UnitStyle.Imperial))
                    {
                        result.Add(UnitType.Fahrenheit);
                    }

                    return result;

                case UnitCategory.HumidityRatio:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(UnitType.KilogramPerKilogram);
                        result.Add(UnitType.GramPerKilogram);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(UnitType.KilogramPerKilogram);
                        result.Add(UnitType.GramPerKilogram);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;

                case UnitCategory.Density:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(UnitType.KilogramPerCubicMeter);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(UnitType.KilogramPerCubicMeter);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;

                case UnitCategory.SpecificVolume:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(UnitType.CubicMeterPerKilogram);
                        result.Add(UnitType.CubicMeterPerGram);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(UnitType.CubicMeterPerKilogram);
                        result.Add(UnitType.CubicMeterPerGram);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;

                case UnitCategory.AirFlow:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(UnitType.CubicMeterPerSecond);
                        result.Add(UnitType.CubicMeterPerHour);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(UnitType.CubicMeterPerSecond);
                        result.Add(UnitType.CubicMeterPerHour);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;
            }

            return null;
        }
    }
}