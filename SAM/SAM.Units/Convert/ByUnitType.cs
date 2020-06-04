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
            }

            return double.NaN;
        }
    }
}