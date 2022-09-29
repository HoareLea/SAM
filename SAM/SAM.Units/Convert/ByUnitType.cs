namespace SAM.Units
{
    public static partial class Convert
    {
        public static double ByUnitType(double value, UnitType from, UnitType to)
        {
            if(from == to)
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
                    switch(to)
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
                    switch(to)
                    {
                        case UnitType.Kelvin:
                            return ByUnitType(ByUnitType(value, from, UnitType.Celsius), UnitType.Celsius, to);

                        case UnitType.Celsius:
                            return (value - 32) / 18;
                    }
                    break;

                case UnitType.KilogramPerKilogram:
                    switch(to)
                    {
                        case UnitType.GramPerKilogram:
                            return value * 1000;
                    }
                    break;

                case UnitType.GramPerKilogram:
                    switch(to)
                    {
                        case UnitType.KilogramPerKilogram:
                            return value / 1000;
                    }
                    break;

                case UnitType.Percent:
                    switch(to)
                    {
                        case UnitType.Unitless:
                            return value / 100;
                    }
                    break;

                case UnitType.Unitless:
                    switch(to)
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

            return double.NaN;
        }

        public static double ByUnitType(double value, UnitType from, UnitStyle unitStyle)
        {
            if(double.IsNaN(value))
            {
                return double.NaN;
            }

            if(from == UnitType.Undefined || unitStyle == UnitStyle.Undefined)
            {
                return value;
            }

            switch(unitStyle)
            {
                case UnitStyle.Imperial:
                    return ToImperial(value, from);

                case UnitStyle.SI:
                    return ToSI(value, from);
            }

            return double.NaN;
        }
    }
}