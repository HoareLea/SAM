﻿using System.Collections.Generic;
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
                        result.Add(Units.UnitType.Celsius);
                        result.Add(Units.UnitType.Fahrenheit);
                    }
                    else if(unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.Celsius);
                    }
                    else if(unitStyles.Contains(UnitStyle.Imperial))
                    {
                        result.Add(Units.UnitType.Fahrenheit);
                    }

                    return result;

                case UnitCategory.HumidityRatio:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.KilogramPerKilogram);
                        result.Add(Units.UnitType.GramPerKilogram);
                        result.Add(Units.UnitType.GramPerGram);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.KilogramPerKilogram);
                        result.Add(Units.UnitType.GramPerKilogram);
                        result.Add(Units.UnitType.GramPerGram);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;

                case UnitCategory.Density:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.KilogramPerCubicMeter);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.KilogramPerCubicMeter);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;

                case UnitCategory.SpecificVolume:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.CubicMeterPerKilogram);
                        result.Add(Units.UnitType.CubicMeterPerGram);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.CubicMeterPerKilogram);
                        result.Add(Units.UnitType.CubicMeterPerGram);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;

                case UnitCategory.AirFlow:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.CubicMeterPerSecond);
                        result.Add(Units.UnitType.CubicMeterPerHour);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.CubicMeterPerSecond);
                        result.Add(Units.UnitType.CubicMeterPerHour);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;


                case UnitCategory.Pressure:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.Pascal);
                        result.Add(Units.UnitType.Kilopascal);
                        result.Add(Units.UnitType.Bar);
                        result.Add(Units.UnitType.PoundPerSquareInch);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.Pascal);
                        result.Add(Units.UnitType.Kilopascal);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {
                        result.Add(Units.UnitType.PoundPerSquareInch);
                    }

                    return result;

                case UnitCategory.Efficiency:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.Unitless);
                        result.Add(Units.UnitType.Percent);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.Unitless);
                        result.Add(Units.UnitType.Percent);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {
                        result.Add(Units.UnitType.Unitless);
                        result.Add(Units.UnitType.Percent);
                    }

                    return result;

                case UnitCategory.Enthaply:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.Jule);
                        result.Add(Units.UnitType.Kilojule);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.Jule);
                        result.Add(Units.UnitType.Kilojule);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;

                case UnitCategory.SpecificEnthaply:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.JulePerKilogram);
                        result.Add(Units.UnitType.KilojulePerKilogram);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.JulePerKilogram);
                        result.Add(Units.UnitType.KilojulePerKilogram);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {

                    }

                    return result;

                case UnitCategory.Power:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.Watt);
                        result.Add(Units.UnitType.Kilowatt);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.Watt);
                        result.Add(Units.UnitType.Kilowatt);
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