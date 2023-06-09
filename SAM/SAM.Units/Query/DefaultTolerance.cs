namespace SAM.Units
{
    public static partial class Query
    {
        public static double DefaultTolerance(this UnitType unitType)
        {
            switch(unitType)
            {
                case Units.UnitType.Celsius:
                    return 0.01;

                case Units.UnitType.Kelvin:
                    return 0.01;

                case Units.UnitType.Pascal:
                    return 0.1;

                case Units.UnitType.GramPerKilogram:
                    return 0.01;

                case Units.UnitType.KilogramPerKilogram:
                    return 0.00001;

                case Units.UnitType.KilojulePerKilogram:
                    return 0.01;

                case Units.UnitType.Percent:
                    return 0.1;

                case Units.UnitType.CubicMeterPerKilogram:
                    return 0.01;

                case Units.UnitType.KilogramPerCubicMeter:
                    return 0.01;
            }

            return double.NaN;
        }
    }
}