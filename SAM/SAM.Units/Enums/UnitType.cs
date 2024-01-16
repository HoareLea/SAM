using System.ComponentModel;

namespace SAM.Units
{
    [Description("UnitType")]
    public enum UnitType
    {
        [Abbreviation("")] [Description("Undefined")] Undefined,
        [Abbreviation("m")] [Description("Meter")] Meter,
        [Abbreviation("ft")] [Description("Feet")] Feet,
        [Abbreviation("°C")] [Description("Celsius")] Celsius,
        [Abbreviation("K")] [Description("Kelvin")] Kelvin,
        [Abbreviation("F")] [Description("Fahrenheit")] Fahrenheit,
        [Abbreviation("kg/kg")] [Description("Kilogram Per Kilogram")] KilogramPerKilogram,
        [Abbreviation("g/kg")] [Description("Gram Per Kilogram")] GramPerKilogram,
        [Abbreviation("%")] [Description("Percent")] Percent,
        [Abbreviation("-")] [Description("Unitless")] Unitless,
        [Abbreviation("kg/m3")] [Description("Kilogram Per Cubic Meter")] KilogramPerCubicMeter,
        [Abbreviation("m3/kg")] [Description("Cubic Meter Per Kilogram")] CubicMeterPerKilogram,
        [Abbreviation("m3/g")] [Description("Cubic Meter Per gram")] CubicMeterPerGram,
        [Abbreviation("m3/h")] [Description("Cubic Meter Per Hour")] CubicMeterPerHour,
        [Abbreviation("m3/s")] [Description("Cubic Meter Per Second")] CubicMeterPerSecond,
        [Abbreviation("Pa")] [Description("Pascal")] Pascal,
        [Abbreviation("kPa")] [Description("Kilopascal")] Kilopascal,
        [Abbreviation("Ba")] [Description("Bar")] Bar,
        [Abbreviation("psi")] [Description("Pound Per Square Inch")] PoundPerSquareInch,
        [Abbreviation("kJ")] [Description("Kilojule")] Kilojule,
        [Abbreviation("kJ/kg")] [Description("Kilojule Per Kilogram")] KilojulePerKilogram,
        [Abbreviation("J/kg")] [Description("Jule Per Kilogram")] JulePerKilogram,
        [Abbreviation("J")] [Description("Jule")] Jule,
        [Abbreviation("W")] [Description("Watt")] Watt,
        [Abbreviation("kW")] [Description("Kliowatt")] Kilowatt,
        [Abbreviation("l/s")][Description("Liters Per Second")] LitersPerSecond,
        [Abbreviation("N/m2")][Description("Newton Per Squere Meter")] NewtonPerSquereMeter,
        [Abbreviation("g/g")][Description("Gram Per Gram")] GramPerGram,
    }
}