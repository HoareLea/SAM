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
            }

            return double.NaN;
        }
    }
}