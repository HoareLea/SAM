// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Units
{
    public static partial class Convert
    {
        public static double ToImperial(double value, UnitType from)
        {
            switch (from)
            {
                case UnitType.Meter:
                    return ByUnitType(value, from, UnitType.Feet);

                case UnitType.Feet:
                    return value;

                case UnitType.Fahrenheit:
                    return value;

                case UnitType.Kelvin:
                    return ByUnitType(value, from, UnitType.Fahrenheit);

                case UnitType.Celsius:
                    return ByUnitType(value, from, UnitType.Fahrenheit);

                case UnitType.Percent:
                    return ByUnitType(value, from, UnitType.Unitless);

                case UnitType.Unitless:
                    return value;

                case UnitType.PoundPerSquareInch:
                    return value;

                case UnitType.Pascal:
                    return ByUnitType(value, from, UnitType.PoundPerSquareInch);

                case UnitType.Kilopascal:
                    return ByUnitType(value, from, UnitType.PoundPerSquareInch);

                case UnitType.Bar:
                    return ByUnitType(value, from, UnitType.PoundPerSquareInch);

                case UnitType.NewtonPerSquereMeter:
                    return ByUnitType(value, from, UnitType.PoundPerSquareInch);

                case UnitType.CubicFootPerMinute:
                    return value;

                case UnitType.CubicMeterPerSecond:
                case UnitType.CubicMeterPerHour:
                case UnitType.LitersPerSecond:
                    return ByUnitType(value, from, UnitType.CubicFootPerMinute);

                case UnitType.BtuPerHour:
                    return value;

                case UnitType.Watt:
                case UnitType.Kilowatt:
                case UnitType.KiloBtuPerHour:
                    return ByUnitType(value, from, UnitType.BtuPerHour);

                case UnitType.SquareFoot:
                    return value;

                case UnitType.SquareMeter:
                    return ByUnitType(value, from, UnitType.SquareFoot);

                case UnitType.CubicFoot:
                    return value;

                case UnitType.CubicMeter:
                    return ByUnitType(value, from, UnitType.CubicFoot);

                case UnitType.BtuPerHourSquareFoot:
                    return value;

                case UnitType.WattPerSquareMeter:
                    return ByUnitType(value, from, UnitType.BtuPerHourSquareFoot);

                case UnitType.FahrenheitDifference:
                    return value;

                case UnitType.KelvinDifference:
                    return ByUnitType(value, from, UnitType.FahrenheitDifference);

                case UnitType.BtuPerHourPerPerson:
                    return value;

                case UnitType.WattPerPerson:
                    return ByUnitType(value, from, UnitType.BtuPerHourPerPerson);

                case UnitType.SquareFootPerPerson:
                    return value;

                case UnitType.SquareMeterPerPerson:
                    return ByUnitType(value, from, UnitType.SquareFootPerPerson);

                case UnitType.AirChangesPerHour:
                    return value;

                case UnitType.FootCandle:
                    return value;

                case UnitType.Lux:
                    return ByUnitType(value, from, UnitType.FootCandle);

                case UnitType.BtuPerHourSquareFootFahrenheit:
                    return value;

                case UnitType.WattPerSquareMeterKelvin:
                    return ByUnitType(value, from, UnitType.BtuPerHourSquareFootFahrenheit);

                case UnitType.Person:
                    return value;

                case UnitType.Hour:
                    return value;

                case UnitType.Degree:
                    return value;

                case UnitType.Radian:
                    return ByUnitType(value, from, UnitType.Degree);
            }

            return double.NaN;
        }
    }
}
