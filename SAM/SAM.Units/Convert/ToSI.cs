// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Units
{
    public static partial class Convert
    {
        public static double ToSI(double value, UnitType from)
        {
            switch (from)
            {
                case UnitType.Feet:
                    return ByUnitType(value, from, UnitType.Meter);

                case UnitType.Meter:
                    return value;

                case UnitType.Celsius:
                    return ByUnitType(value, from, UnitType.Kelvin);

                case UnitType.Fahrenheit:
                    return ByUnitType(value, from, UnitType.Kelvin);

                case UnitType.Kelvin:
                    return value;

                case UnitType.KilogramPerKilogram:
                    return value;

                case UnitType.GramPerKilogram:
                    return ByUnitType(value, from, UnitType.KilogramPerKilogram);

                case UnitType.Percent:
                    return ByUnitType(value, from, UnitType.Unitless);

                case UnitType.Unitless:
                    return value;

                case UnitType.CubicMeterPerSecond:
                    return value;

                case UnitType.CubicMeterPerHour:
                    return ByUnitType(value, from, UnitType.CubicMeterPerSecond);

                case UnitType.CubicMeterPerGram:
                    return ByUnitType(value, from, UnitType.CubicMeterPerKilogram);

                case UnitType.CubicMeterPerKilogram:
                    return value;

                case UnitType.Bar:
                    return ByUnitType(value, from, UnitType.Pascal);

                case UnitType.Kilopascal:
                    return ByUnitType(value, from, UnitType.Pascal);

                case UnitType.Pascal:
                    return value;

                case UnitType.PoundPerSquareInch:
                    return ByUnitType(value, from, UnitType.Pascal);

                case UnitType.Kilojule:
                    return ByUnitType(value, from, UnitType.Jule);

                case UnitType.Jule:
                    return value;

                case UnitType.GramPerGram:
                    return ByUnitType(value, from, UnitType.KilogramPerKilogram);

                case UnitType.KilogramPerCubicMeter:
                    return value;

                case UnitType.NewtonPerSquereMeter:
                    return ByUnitType(value, from, UnitType.Pascal);

                case UnitType.KilojulePerKilogram:
                    return ByUnitType(value, from, UnitType.JulePerKilogram);

                case UnitType.JulePerKilogram:
                    return value;

                case UnitType.LitersPerSecond:
                case UnitType.CubicFootPerMinute:
                    return ByUnitType(value, from, UnitType.CubicMeterPerSecond);

                case UnitType.Watt:
                    return value;

                case UnitType.Kilowatt:
                case UnitType.BtuPerHour:
                case UnitType.KiloBtuPerHour:
                    return ByUnitType(value, from, UnitType.Watt);

                case UnitType.SquareMeter:
                    return value;

                case UnitType.SquareFoot:
                    return ByUnitType(value, from, UnitType.SquareMeter);

                case UnitType.CubicMeter:
                    return value;

                case UnitType.CubicFoot:
                    return ByUnitType(value, from, UnitType.CubicMeter);

                case UnitType.WattPerSquareMeter:
                    return value;

                case UnitType.BtuPerHourSquareFoot:
                    return ByUnitType(value, from, UnitType.WattPerSquareMeter);

                case UnitType.KelvinDifference:
                    return value;

                case UnitType.FahrenheitDifference:
                    return ByUnitType(value, from, UnitType.KelvinDifference);

                case UnitType.WattPerPerson:
                    return value;

                case UnitType.BtuPerHourPerPerson:
                    return ByUnitType(value, from, UnitType.WattPerPerson);

                case UnitType.SquareMeterPerPerson:
                    return value;

                case UnitType.SquareFootPerPerson:
                    return ByUnitType(value, from, UnitType.SquareMeterPerPerson);

                case UnitType.AirChangesPerHour:
                    return value;

                case UnitType.Lux:
                    return value;

                case UnitType.FootCandle:
                    return ByUnitType(value, from, UnitType.Lux);

                case UnitType.WattPerSquareMeterKelvin:
                    return value;

                case UnitType.BtuPerHourSquareFootFahrenheit:
                    return ByUnitType(value, from, UnitType.WattPerSquareMeterKelvin);

                case UnitType.Person:
                    return value;

                case UnitType.Hour:
                    return value;

                case UnitType.Radian:
                    return value;

                case UnitType.Degree:
                    return ByUnitType(value, from, UnitType.Radian);
            }

            return double.NaN;
        }
    }
}
