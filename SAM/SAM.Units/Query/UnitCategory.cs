// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Units
{
    public static partial class Query
    {
        /// <summary>
        /// Primary category of a unit type. A unit type shared by several categories maps to one of them:
        /// Unitless and Percent map to Ratio (not Efficiency or RelativeHumidity), and Kelvin maps to Temperature
        /// (a temperature difference uses KelvinDifference).
        /// </summary>
        public static Units.UnitCategory UnitCategory(this UnitType unitType)
        {
            switch (unitType)
            {
                case Units.UnitType.Meter:
                case Units.UnitType.Feet:
                    return Units.UnitCategory.Length;

                case Units.UnitType.Celsius:
                case Units.UnitType.Kelvin:
                case Units.UnitType.Fahrenheit:
                    return Units.UnitCategory.Temperature;

                case Units.UnitType.KilogramPerKilogram:
                case Units.UnitType.GramPerKilogram:
                case Units.UnitType.GramPerGram:
                    return Units.UnitCategory.HumidityRatio;

                case Units.UnitType.Percent:
                case Units.UnitType.Unitless:
                    return Units.UnitCategory.Ratio;

                case Units.UnitType.KilogramPerCubicMeter:
                    return Units.UnitCategory.Density;

                case Units.UnitType.CubicMeterPerKilogram:
                case Units.UnitType.CubicMeterPerGram:
                    return Units.UnitCategory.SpecificVolume;

                case Units.UnitType.CubicMeterPerHour:
                case Units.UnitType.CubicMeterPerSecond:
                case Units.UnitType.LitersPerSecond:
                case Units.UnitType.CubicFootPerMinute:
                    return Units.UnitCategory.AirFlow;

                case Units.UnitType.Pascal:
                case Units.UnitType.Kilopascal:
                case Units.UnitType.Bar:
                case Units.UnitType.PoundPerSquareInch:
                case Units.UnitType.NewtonPerSquereMeter:
                    return Units.UnitCategory.Pressure;

                case Units.UnitType.Kilojule:
                case Units.UnitType.Jule:
                    return Units.UnitCategory.Enthaply;

                case Units.UnitType.KilojulePerKilogram:
                case Units.UnitType.JulePerKilogram:
                    return Units.UnitCategory.SpecificEnthaply;

                case Units.UnitType.Watt:
                case Units.UnitType.Kilowatt:
                case Units.UnitType.BtuPerHour:
                case Units.UnitType.KiloBtuPerHour:
                    return Units.UnitCategory.Power;

                case Units.UnitType.SquareMeter:
                case Units.UnitType.SquareFoot:
                    return Units.UnitCategory.Area;

                case Units.UnitType.CubicMeter:
                case Units.UnitType.CubicFoot:
                    return Units.UnitCategory.Volume;

                case Units.UnitType.WattPerSquareMeter:
                case Units.UnitType.BtuPerHourSquareFoot:
                    return Units.UnitCategory.SpecificPower;

                case Units.UnitType.KelvinDifference:
                case Units.UnitType.FahrenheitDifference:
                    return Units.UnitCategory.TemperatureDifference;

                case Units.UnitType.WattPerPerson:
                case Units.UnitType.BtuPerHourPerPerson:
                    return Units.UnitCategory.PowerPerPerson;

                case Units.UnitType.SquareMeterPerPerson:
                case Units.UnitType.SquareFootPerPerson:
                    return Units.UnitCategory.AreaPerPerson;

                case Units.UnitType.AirChangesPerHour:
                    return Units.UnitCategory.AirChangeRate;

                case Units.UnitType.Lux:
                case Units.UnitType.FootCandle:
                    return Units.UnitCategory.Illuminance;

                case Units.UnitType.WattPerSquareMeterKelvin:
                case Units.UnitType.BtuPerHourSquareFootFahrenheit:
                    return Units.UnitCategory.ThermalTransmittance;

                case Units.UnitType.Person:
                    return Units.UnitCategory.Count;

                case Units.UnitType.Hour:
                    return Units.UnitCategory.Time;

                case Units.UnitType.Degree:
                case Units.UnitType.Radian:
                    return Units.UnitCategory.Angle;
            }

            return Units.UnitCategory.Undefined;
        }
    }
}
