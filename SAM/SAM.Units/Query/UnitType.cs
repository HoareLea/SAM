using System;
using System.Collections.Generic;

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

                        case UnitCategory.Efficiency:
                            return Units.UnitType.Unitless;

                        case UnitCategory.Undefined:
                            return Units.UnitType.Undefined;

                        case UnitCategory.Enthaply:
                            return Units.UnitType.Jule;

                        case UnitCategory.SpecificEnthaply:
                            return Units.UnitType.JulePerKilogram;

                        case UnitCategory.Power:
                            return Units.UnitType.Watt;
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

        public static UnitType UnitType(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Units.UnitType.Undefined;

            Array array = Enum.GetValues(typeof(UnitType));
            if (array == null || array.Length == 0)
                return Units.UnitType.Undefined;

            foreach (UnitType unitType in array)
                if (unitType.ToString().Equals(text))
                    return unitType;

            List<string> texts = new List<string>();
            string text_Temp = null;

            foreach (UnitType unitType in array)
            {
                text_Temp = unitType.Abbreviation();
                texts.Add(text_Temp);
                if (text_Temp.Equals(text))
                    return unitType;

                text_Temp = unitType.Description();
                texts.Add(text_Temp);
                if (text_Temp.Equals(text))
                    return unitType;
            }

            text_Temp = text.ToUpper().Replace(" ", string.Empty);
            for (int i = 0; i < array.Length; i++)
            {
                if (texts[i].ToUpper().Replace(" ", string.Empty).Equals(text_Temp))
                    return (UnitType)array.GetValue(i);
            }

            if (text.Equals("Undefined"))
                return default;

            return Units.UnitType.Undefined;
        }
    }
}