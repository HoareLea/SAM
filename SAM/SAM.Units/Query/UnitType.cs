namespace SAM.Units
{
    public static partial class Query
    {
        public static UnitType UnitType(this UnitStyle unitStyle, UnitCategory unitCategory)
        {
            if(unitStyle == UnitStyle.Undefined || unitCategory == UnitCategory.Undefined)
            {
                return Units.UnitType.Undefined;
            }

            switch(unitStyle)
            {
                case UnitStyle.SI:
                    switch(unitCategory)
                    {
                        case UnitCategory.AirFlow:
                            return Units.UnitType.CubicMeterPerSecond;

                        case UnitCategory.Density:
                            return Units.UnitType.KilogramPerCubicMeter;

                        case UnitCategory.HumidityRatio:
                            return Units.UnitType.KilogramPerKilogram;

                        case UnitCategory.RelativeHumidity:
                            return Units.UnitType.Unitless;

                        case UnitCategory.Pressure:
                            return Units.UnitType.Pascal;

                        case UnitCategory.SpecificVolume:
                            return Units.UnitType.CubicMeterPerKilogram;

                        case UnitCategory.Temperature:
                            return Units.UnitType.Celsius;

                        case UnitCategory.Undefined:
                            return Units.UnitType.Undefined;
                    }
                    break;

                case UnitStyle.Imperial:
                    switch (unitCategory)
                    {
                        case UnitCategory.Pressure:
                            return Units.UnitType.PoundPerSquareInch;
                    }
                    break;
            }

            return Units.UnitType.Undefined;
        }
    }
}