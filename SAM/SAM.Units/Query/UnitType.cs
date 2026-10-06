// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Units
{
    public static partial class Query
    {
        public static UnitType UnitType(this UnitStyle unitStyle, Units.UnitCategory unitCategory)
        {
            if (unitStyle == UnitStyle.Undefined || unitCategory == Units.UnitCategory.Undefined)
            {
                return Units.UnitType.Undefined;
            }

            switch (unitStyle)
            {
                case UnitStyle.SI:
                    switch (unitCategory)
                    {
                        case Units.UnitCategory.AirFlow:
                            return Units.UnitType.CubicMeterPerSecond;

                        case Units.UnitCategory.Density:
                            return Units.UnitType.KilogramPerCubicMeter;

                        case Units.UnitCategory.HumidityRatio:
                            return Units.UnitType.KilogramPerKilogram;

                        case Units.UnitCategory.RelativeHumidity:
                            return Units.UnitType.Unitless;

                        case Units.UnitCategory.Pressure:
                            return Units.UnitType.Pascal;

                        case Units.UnitCategory.SpecificVolume:
                            return Units.UnitType.CubicMeterPerKilogram;

                        case Units.UnitCategory.Temperature:
                            return Units.UnitType.Celsius;

                        case Units.UnitCategory.Efficiency:
                            return Units.UnitType.Unitless;

                        case Units.UnitCategory.Undefined:
                            return Units.UnitType.Undefined;

                        case Units.UnitCategory.Enthaply:
                            return Units.UnitType.Jule;

                        case Units.UnitCategory.SpecificEnthaply:
                            return Units.UnitType.JulePerKilogram;

                        case Units.UnitCategory.Power:
                            return Units.UnitType.Watt;

                        case Units.UnitCategory.Area:
                            return Units.UnitType.SquareMeter;

                        case Units.UnitCategory.Volume:
                            return Units.UnitType.CubicMeter;

                        case Units.UnitCategory.Length:
                            return Units.UnitType.Meter;

                        case Units.UnitCategory.TemperatureDifference:
                            return Units.UnitType.KelvinDifference;

                        case Units.UnitCategory.SpecificPower:
                            return Units.UnitType.WattPerSquareMeter;

                        case Units.UnitCategory.PowerPerPerson:
                            return Units.UnitType.WattPerPerson;

                        case Units.UnitCategory.AreaPerPerson:
                            return Units.UnitType.SquareMeterPerPerson;

                        case Units.UnitCategory.Illuminance:
                            return Units.UnitType.Lux;

                        case Units.UnitCategory.ThermalTransmittance:
                            return Units.UnitType.WattPerSquareMeterKelvin;
                    }
                    break;

                case UnitStyle.Imperial:
                    switch (unitCategory)
                    {
                        case Units.UnitCategory.Pressure:
                            return Units.UnitType.PoundPerSquareInch;

                        case Units.UnitCategory.Temperature:
                            return Units.UnitType.Fahrenheit;

                        case Units.UnitCategory.AirFlow:
                            return Units.UnitType.CubicFootPerMinute;

                        case Units.UnitCategory.Power:
                            return Units.UnitType.BtuPerHour;

                        case Units.UnitCategory.Area:
                            return Units.UnitType.SquareFoot;

                        case Units.UnitCategory.Volume:
                            return Units.UnitType.CubicFoot;

                        case Units.UnitCategory.Length:
                            return Units.UnitType.Feet;

                        case Units.UnitCategory.TemperatureDifference:
                            return Units.UnitType.FahrenheitDifference;

                        case Units.UnitCategory.SpecificPower:
                            return Units.UnitType.BtuPerHourSquareFoot;

                        case Units.UnitCategory.PowerPerPerson:
                            return Units.UnitType.BtuPerHourPerPerson;

                        case Units.UnitCategory.AreaPerPerson:
                            return Units.UnitType.SquareFootPerPerson;

                        case Units.UnitCategory.Illuminance:
                            return Units.UnitType.FootCandle;

                        case Units.UnitCategory.ThermalTransmittance:
                            return Units.UnitType.BtuPerHourSquareFootFahrenheit;
                    }
                    break;
            }

            // Categories whose unit is the same in both styles
            switch (unitCategory)
            {
                case Units.UnitCategory.AirChangeRate:
                    return Units.UnitType.AirChangesPerHour;

                case Units.UnitCategory.Time:
                    return Units.UnitType.Hour;

                case Units.UnitCategory.Count:
                    return Units.UnitType.Person;

                case Units.UnitCategory.Ratio:
                    return Units.UnitType.Unitless;

                case Units.UnitCategory.Angle:
                    return Units.UnitType.Degree;
            }

            return Units.UnitType.Undefined;
        }

        public static UnitType UnitType(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Units.UnitType.Undefined;

            Array array = Enum.GetValues(typeof(UnitType));
            if (array == null || array.Length == 0)
                return Units.UnitType.Undefined;

            foreach (UnitType unitType in array)
                if (unitType.ToString().Equals(text))
                    return unitType;

            // Each candidate text keeps its own unit type, so the case-insensitive pass below maps a text back to
            // the unit it came from (two texts per unit type: abbreviation, then description).
            List<KeyValuePair<string, UnitType>> texts = new List<KeyValuePair<string, UnitType>>();
            string text_Temp = null;

            foreach (UnitType unitType in array)
            {
                text_Temp = unitType.Abbreviation();
                texts.Add(new KeyValuePair<string, UnitType>(text_Temp, unitType));
                if (text_Temp.Equals(text))
                    return unitType;

                text_Temp = unitType.Description();
                texts.Add(new KeyValuePair<string, UnitType>(text_Temp, unitType));
                if (text_Temp.Equals(text))
                    return unitType;
            }

            text_Temp = text.ToUpperInvariant().Replace(" ", string.Empty);
            foreach (KeyValuePair<string, UnitType> keyValuePair in texts)
            {
                if (keyValuePair.Key.ToUpperInvariant().Replace(" ", string.Empty).Equals(text_Temp))
                    return keyValuePair.Value;
            }

            if (text.Equals("Undefined"))
                return default;

            return Units.UnitType.Undefined;
        }
    }
}
