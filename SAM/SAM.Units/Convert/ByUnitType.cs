// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Units
{
    public static partial class Convert
    {
        public static double ByUnitType(double value, UnitType from, UnitType to)
        {
            if (from == to)
            {
                return value;
            }


            switch (from)
            {
                case UnitType.Meter:
                    switch (to)
                    {
                        case UnitType.Feet:
                            return value * Factor.MetersToFeet;
                    }
                    break;

                case UnitType.Feet:
                    switch (to)
                    {
                        case UnitType.Meter:
                            return value * Factor.FeetToMeters;
                    }
                    break;

                case UnitType.Kelvin:
                    switch (to)
                    {
                        case UnitType.Celsius:
                            return value + Factor.KelvinToCelsius;

                        case UnitType.Fahrenheit:
                            return ByUnitType(ByUnitType(value, from, UnitType.Celsius), UnitType.Celsius, to);
                    }
                    break;

                case UnitType.Celsius:
                    switch (to)
                    {
                        case UnitType.Kelvin:
                            return value + Factor.CelsisToKelvin;

                        case UnitType.Fahrenheit:
                            return (1.8 * value) + 32;
                    }
                    break;

                case UnitType.Fahrenheit:
                    switch (to)
                    {
                        case UnitType.Kelvin:
                            return ByUnitType(ByUnitType(value, from, UnitType.Celsius), UnitType.Celsius, to);

                        case UnitType.Celsius:
                            return (value - 32) / 1.8;
                    }
                    break;

                case UnitType.KilogramPerKilogram:
                    switch (to)
                    {
                        case UnitType.GramPerKilogram:
                            return value * 1000;

                        case UnitType.GramPerGram:
                            return value;
                    }
                    break;

                case UnitType.GramPerGram:
                    switch (to)
                    {
                        case UnitType.GramPerKilogram:
                            return value * 1000;
                    }
                    break;

                case UnitType.GramPerKilogram:
                    switch (to)
                    {
                        case UnitType.KilogramPerKilogram:
                            return value / 1000;

                        case UnitType.GramPerGram:
                            return value / 1000;
                    }
                    break;

                case UnitType.Percent:
                    switch (to)
                    {
                        case UnitType.Unitless:
                            return value / 100;
                    }
                    break;

                case UnitType.Unitless:
                    switch (to)
                    {
                        case UnitType.Percent:
                            return value * 100;
                    }
                    break;

                case UnitType.CubicMeterPerHour:
                    switch (to)
                    {
                        case UnitType.CubicMeterPerSecond:
                            return value / 3600;
                    }
                    break;

                case UnitType.CubicMeterPerSecond:
                    switch (to)
                    {
                        case UnitType.CubicMeterPerHour:
                            return value * 3600;
                    }
                    break;

                case UnitType.Pascal:
                    switch (to)
                    {
                        case UnitType.Bar:
                            return value / 100000;

                        case UnitType.Kilopascal:
                            return value / 1000;

                        case UnitType.PoundPerSquareInch:
                            return value * Factor.PascalToPoundsPerInch;
                    }
                    break;

                case UnitType.Kilopascal:
                    switch (to)
                    {
                        case UnitType.Bar:
                            return value / 100;

                        case UnitType.Pascal:
                            return value * 1000;

                        case UnitType.PoundPerSquareInch:
                            return ByUnitType(value, from, UnitType.Pascal) * Factor.PascalToPoundsPerInch;
                    }
                    break;

                case UnitType.Bar:
                    switch (to)
                    {
                        case UnitType.Kilopascal:
                            return value * 100;

                        case UnitType.Pascal:
                            return value * 100000;

                        case UnitType.PoundPerSquareInch:
                            return ByUnitType(value, from, UnitType.Pascal) * Factor.PascalToPoundsPerInch;
                    }
                    break;

                case UnitType.PoundPerSquareInch:
                    switch (to)
                    {
                        case UnitType.Bar:
                            return ByUnitType(ByUnitType(value, from, UnitType.Pascal), UnitType.Pascal, UnitType.Bar);

                        case UnitType.Kilopascal:
                            return ByUnitType(ByUnitType(value, from, UnitType.Pascal), UnitType.Pascal, UnitType.Kilopascal);

                        case UnitType.Pascal:
                            return value * Factor.PoundsPerInchToPascal;
                    }
                    break;

                case UnitType.Jule:
                    switch (to)
                    {
                        case UnitType.Kilojule:
                            return value / 1000;
                    }
                    break;

                case UnitType.Kilojule:
                    switch (to)
                    {
                        case UnitType.Jule:
                            return value * 1000;
                    }
                    break;
            }

            return ByLinearFactor(value, from, to);
        }

        public static double ByUnitType(double value, UnitType from, UnitStyle unitStyle)
        {
            if (double.IsNaN(value))
            {
                return double.NaN;
            }

            if (from == UnitType.Undefined || unitStyle == UnitStyle.Undefined)
            {
                return value;
            }

            switch (unitStyle)
            {
                case UnitStyle.Imperial:
                    return ToImperial(value, from);

                case UnitStyle.SI:
                    return ToSI(value, from);
            }

            return double.NaN;
        }

        /// <summary>
        /// Linear (offset-free) conversion between two unit types of the same family, used for pairs the explicit
        /// switch in <see cref="ByUnitType(double, UnitType, UnitType)"/> does not handle. The family of a unit type
        /// is its <see cref="Query.UnitCategory(UnitType)"/>, and each factor converts one unit into the family's base
        /// unit. Absolute temperatures are affine and deliberately absent: they stay in the explicit switch.
        /// </summary>
        private static double ByLinearFactor(double value, UnitType from, UnitType to)
        {
            if (!linearFactors.TryGetValue(from, out double factor_From) || !linearFactors.TryGetValue(to, out double factor_To))
            {
                return double.NaN;
            }

            if (Query.UnitCategory(from) != Query.UnitCategory(to))
            {
                return double.NaN;
            }

            return value * factor_From / factor_To;
        }

        private static readonly Dictionary<UnitType, double> linearFactors = new Dictionary<UnitType, double>()
        {
            // Length [m]
            { UnitType.Meter, 1 },
            { UnitType.Feet, Factor.FeetToMeters },

            // Area [m2]
            { UnitType.SquareMeter, 1 },
            { UnitType.SquareFoot, Factor.SquareFeetToSquareMeters },

            // Volume [m3]
            { UnitType.CubicMeter, 1 },
            { UnitType.CubicFoot, Factor.CubicFeetToCubicMeters },

            // AirFlow [m3/s]
            { UnitType.CubicMeterPerSecond, 1 },
            { UnitType.CubicMeterPerHour, 1.0 / 3600 },
            { UnitType.LitersPerSecond, 0.001 },
            { UnitType.CubicFootPerMinute, Factor.CubicFeetPerMinuteToCubicMetersPerSecond },

            // Power [W]
            { UnitType.Watt, 1 },
            { UnitType.Kilowatt, 1000 },
            { UnitType.BtuPerHour, Factor.BtuPerHourToWatts },
            { UnitType.KiloBtuPerHour, 1000 * Factor.BtuPerHourToWatts },

            // SpecificPower [W/m2]
            { UnitType.WattPerSquareMeter, 1 },
            { UnitType.BtuPerHourSquareFoot, Factor.BtuPerHourSquareFootToWattsPerSquareMeter },

            // TemperatureDifference [K] - a difference has no offset
            { UnitType.KelvinDifference, 1 },
            { UnitType.FahrenheitDifference, Factor.FahrenheitDifferenceToKelvinDifference },

            // PowerPerPerson [W/person]
            { UnitType.WattPerPerson, 1 },
            { UnitType.BtuPerHourPerPerson, Factor.BtuPerHourToWatts },

            // AreaPerPerson [m2/person]
            { UnitType.SquareMeterPerPerson, 1 },
            { UnitType.SquareFootPerPerson, Factor.SquareFeetToSquareMeters },

            // AirChangeRate [1/h]
            { UnitType.AirChangesPerHour, 1 },

            // Illuminance [lx]
            { UnitType.Lux, 1 },
            { UnitType.FootCandle, Factor.FootCandlesToLux },

            // ThermalTransmittance [W/m2K]
            { UnitType.WattPerSquareMeterKelvin, 1 },
            { UnitType.BtuPerHourSquareFootFahrenheit, Factor.BtuPerHourSquareFootFahrenheitToWattsPerSquareMeterKelvin },

            // Time [h]
            { UnitType.Hour, 1 },

            // Count [person]
            { UnitType.Person, 1 },

            // Ratio [-]
            { UnitType.Unitless, 1 },
            { UnitType.Percent, 0.01 },

            // Angle [rad]
            { UnitType.Radian, 1 },
            { UnitType.Degree, Factor.DegreesToRadians },

            // HumidityRatio [kg/kg]
            { UnitType.KilogramPerKilogram, 1 },
            { UnitType.GramPerKilogram, 0.001 },
            { UnitType.GramPerGram, 1 },

            // SpecificVolume [m3/kg]
            { UnitType.CubicMeterPerKilogram, 1 },
            { UnitType.CubicMeterPerGram, 1000 },

            // Pressure [Pa]
            { UnitType.Pascal, 1 },
            { UnitType.Kilopascal, 1000 },
            { UnitType.Bar, 100000 },
            { UnitType.PoundPerSquareInch, Factor.PoundsPerInchToPascal },
            { UnitType.NewtonPerSquereMeter, 1 },

            // Enthaply [J]
            { UnitType.Jule, 1 },
            { UnitType.Kilojule, 1000 },

            // SpecificEnthaply [J/kg]
            { UnitType.JulePerKilogram, 1 },
            { UnitType.KilojulePerKilogram, 1000 },
        };
    }
}
