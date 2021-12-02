namespace SAM.Units
{
    public static partial class Convert
    {
        public static double ByUnitType(double value, UnitType from, UnitType to)
        {
            switch (from)
            {
                case UnitType.Meter:
                    switch (to)
                    {
                        case UnitType.Feet:
                            return value * Factor.MetersToFeet;

                        case UnitType.Meter:
                            return value;
                    }
                    break;

                case UnitType.Feet:
                    switch (to)
                    {
                        case UnitType.Meter:
                            return value * Factor.FeetToMeters;

                        case UnitType.Feet:
                            return value;
                    }
                    break;

                case UnitType.Kelvin:
                    switch(to)
                    {
                        case UnitType.Celsius:
                            return value + Factor.KelvinToCelsius;

                        case UnitType.Fahrenheit:
                            return ByUnitType(ByUnitType(value, from, UnitType.Celsius), UnitType.Celsius, to);

                        case UnitType.Kelvin:
                            return value;
                    }
                    break;

                case UnitType.Celsius:
                    switch (to)
                    {
                        case UnitType.Kelvin:
                            return value + Factor.CelsisToKelvin;

                        case UnitType.Fahrenheit:
                            return (1.8 * value) + 32;

                        case UnitType.Celsius:
                            return value;
                    }
                    break;

                case UnitType.Fahrenheit:
                    switch(to)
                    {
                        case UnitType.Kelvin:
                            return ByUnitType(ByUnitType(value, from, UnitType.Celsius), UnitType.Celsius, to);

                        case UnitType.Celsius:
                            return (value - 32) / 18;

                        case UnitType.Fahrenheit:
                            return value;
                    }
                    break;
            }

            return double.NaN;
        }
    }
}