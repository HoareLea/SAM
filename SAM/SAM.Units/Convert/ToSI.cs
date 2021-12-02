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
            }

            return double.NaN;
        }
    }
}