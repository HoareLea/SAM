// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Units
{
    public static partial class Query
    {
        public static List<UnitType> UnitTypes(this Units.UnitCategory unitCategory, params UnitStyle[] unitStyles)
        {
            List<UnitType> result = new List<UnitType>();
            switch (unitCategory)
            {
                case Units.UnitCategory.Temperature:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.Celsius);
                        result.Add(Units.UnitType.Fahrenheit);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.Celsius);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {
                        result.Add(Units.UnitType.Fahrenheit);
                    }

                    return result;

                case Units.UnitCategory.HumidityRatio:

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

                case Units.UnitCategory.Density:

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

                case Units.UnitCategory.SpecificVolume:

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

                case Units.UnitCategory.AirFlow:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.CubicMeterPerSecond);
                        result.Add(Units.UnitType.CubicMeterPerHour);
                        result.Add(Units.UnitType.LitersPerSecond);
                        result.Add(Units.UnitType.CubicFootPerMinute);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.CubicMeterPerSecond);
                        result.Add(Units.UnitType.CubicMeterPerHour);
                        result.Add(Units.UnitType.LitersPerSecond);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {
                        result.Add(Units.UnitType.CubicFootPerMinute);
                    }

                    return result;


                case Units.UnitCategory.Pressure:

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

                case Units.UnitCategory.Efficiency:

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

                case Units.UnitCategory.Enthaply:

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

                case Units.UnitCategory.SpecificEnthaply:

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

                case Units.UnitCategory.Power:

                    if (unitStyles == null || unitStyles.Length == 0)
                    {
                        result.Add(Units.UnitType.Watt);
                        result.Add(Units.UnitType.Kilowatt);
                        result.Add(Units.UnitType.BtuPerHour);
                        result.Add(Units.UnitType.KiloBtuPerHour);
                    }
                    else if (unitStyles.Contains(UnitStyle.SI))
                    {
                        result.Add(Units.UnitType.Watt);
                        result.Add(Units.UnitType.Kilowatt);
                    }
                    else if (unitStyles.Contains(UnitStyle.Imperial))
                    {
                        result.Add(Units.UnitType.BtuPerHour);
                        result.Add(Units.UnitType.KiloBtuPerHour);
                    }

                    return result;

                case Units.UnitCategory.Area:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.SquareMeter }, new[] { Units.UnitType.SquareFoot });

                case Units.UnitCategory.Volume:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.CubicMeter }, new[] { Units.UnitType.CubicFoot });

                case Units.UnitCategory.Length:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.Meter }, new[] { Units.UnitType.Feet });

                case Units.UnitCategory.TemperatureDifference:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.KelvinDifference }, new[] { Units.UnitType.FahrenheitDifference });

                case Units.UnitCategory.AirChangeRate:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.AirChangesPerHour }, new[] { Units.UnitType.AirChangesPerHour });

                case Units.UnitCategory.SpecificPower:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.WattPerSquareMeter }, new[] { Units.UnitType.BtuPerHourSquareFoot });

                case Units.UnitCategory.PowerPerPerson:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.WattPerPerson }, new[] { Units.UnitType.BtuPerHourPerPerson });

                case Units.UnitCategory.AreaPerPerson:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.SquareMeterPerPerson }, new[] { Units.UnitType.SquareFootPerPerson });

                case Units.UnitCategory.Illuminance:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.Lux }, new[] { Units.UnitType.FootCandle });

                case Units.UnitCategory.ThermalTransmittance:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.WattPerSquareMeterKelvin }, new[] { Units.UnitType.BtuPerHourSquareFootFahrenheit });

                case Units.UnitCategory.Time:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.Hour }, new[] { Units.UnitType.Hour });

                case Units.UnitCategory.Count:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.Person }, new[] { Units.UnitType.Person });

                case Units.UnitCategory.Ratio:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.Unitless, Units.UnitType.Percent }, new[] { Units.UnitType.Unitless, Units.UnitType.Percent });

                case Units.UnitCategory.Angle:
                    return UnitTypes(unitStyles, new[] { Units.UnitType.Degree, Units.UnitType.Radian }, new[] { Units.UnitType.Degree, Units.UnitType.Radian });
            }

            return null;
        }

        /// <summary>
        /// Same selection rule as the explicit cases above: no style gives every unit type (SI first, without
        /// duplicates), SI wins when both styles are given.
        /// </summary>
        private static List<UnitType> UnitTypes(UnitStyle[] unitStyles, UnitType[] unitTypes_SI, UnitType[] unitTypes_Imperial)
        {
            if (unitStyles == null || unitStyles.Length == 0)
            {
                return unitTypes_SI.Concat(unitTypes_Imperial).Distinct().ToList();
            }

            if (unitStyles.Contains(UnitStyle.SI))
            {
                return unitTypes_SI.ToList();
            }

            if (unitStyles.Contains(UnitStyle.Imperial))
            {
                return unitTypes_Imperial.ToList();
            }

            return new List<UnitType>();
        }
    }
}
