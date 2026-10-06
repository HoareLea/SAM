// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Units
{
    public static class Factor
    {
        public const double RadiansToDegrees = 180 / Math.PI;
        public const double DegreesToRadians = 1 / RadiansToDegrees;

        public const double MetersToFeet = 3.280839895;
        public const double FeetToMeters = 1 / MetersToFeet;

        public const double CelsisToKelvin = 273.15;
        public const double KelvinToCelsius = -CelsisToKelvin;

        public const double PoundsPerInchToPascal = 6894.75728;
        public const double PascalToPoundsPerInch = 1 / PoundsPerInchToPascal;

        public const double SquareMetersToSquareFeet = MetersToFeet * MetersToFeet;
        public const double SquareFeetToSquareMeters = 1 / SquareMetersToSquareFeet;

        public const double CubicMetersToCubicFeet = SquareMetersToSquareFeet * MetersToFeet;
        public const double CubicFeetToCubicMeters = 1 / CubicMetersToCubicFeet;

        public const double CubicMetersPerSecondToCubicFeetPerMinute = 2118.880003;
        public const double CubicFeetPerMinuteToCubicMetersPerSecond = 1 / CubicMetersPerSecondToCubicFeetPerMinute;

        public const double WattsToBtuPerHour = 3.412141633;
        public const double BtuPerHourToWatts = 1 / WattsToBtuPerHour;

        public const double WattsPerSquareMeterToBtuPerHourSquareFoot = 0.316998331;
        public const double BtuPerHourSquareFootToWattsPerSquareMeter = 1 / WattsPerSquareMeterToBtuPerHourSquareFoot;

        public const double LuxToFootCandles = 0.09290304;
        public const double FootCandlesToLux = 1 / LuxToFootCandles;

        public const double WattsPerSquareMeterKelvinToBtuPerHourSquareFootFahrenheit = 0.176110184;
        public const double BtuPerHourSquareFootFahrenheitToWattsPerSquareMeterKelvin = 1 / WattsPerSquareMeterKelvinToBtuPerHourSquareFootFahrenheit;

        public const double KelvinDifferenceToFahrenheitDifference = 1.8;
        public const double FahrenheitDifferenceToKelvinDifference = 1 / KelvinDifferenceToFahrenheitDifference;
    }
}
