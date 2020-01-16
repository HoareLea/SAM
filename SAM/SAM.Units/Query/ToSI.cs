namespace SAM.Units
{
    public static partial class Query
    {
        public static double ToSI(double value, UnitType from)
        {
            switch (from)
            {
                case UnitType.Feet:
                    return Convert(value, from, UnitType.Meter);
            }

            return double.NaN;
        }
    }
}
