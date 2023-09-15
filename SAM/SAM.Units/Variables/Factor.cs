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
    }
}
