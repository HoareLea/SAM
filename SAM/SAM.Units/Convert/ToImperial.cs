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
            }

            return double.NaN;
        }
    }
}