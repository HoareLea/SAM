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
            }

            return double.NaN;
        }
    }
}