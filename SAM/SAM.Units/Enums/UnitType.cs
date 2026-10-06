// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Units
{
    [Description("UnitType")]
    public enum UnitType
    {
        [Abbreviation("")][Description("Undefined")] Undefined,
        [Abbreviation("m")][Description("Meter")] Meter,
        [Abbreviation("ft")][Description("Feet")] Feet,
        [Abbreviation("°C")][Description("Celsius")] Celsius,
        [Abbreviation("K")][Description("Kelvin")] Kelvin,
        [Abbreviation("F")][Description("Fahrenheit")] Fahrenheit,
        [Abbreviation("kg/kg")][Description("Kilogram Per Kilogram")] KilogramPerKilogram,
        [Abbreviation("g/kg")][Description("Gram Per Kilogram")] GramPerKilogram,
        [Abbreviation("%")][Description("Percent")] Percent,
        [Abbreviation("-")][Description("Unitless")] Unitless,
        [Abbreviation("kg/m3")][Description("Kilogram Per Cubic Meter")] KilogramPerCubicMeter,
        [Abbreviation("m3/kg")][Description("Cubic Meter Per Kilogram")] CubicMeterPerKilogram,
        [Abbreviation("m3/g")][Description("Cubic Meter Per gram")] CubicMeterPerGram,
        [Abbreviation("m3/h")][Description("Cubic Meter Per Hour")] CubicMeterPerHour,
        [Abbreviation("m3/s")][Description("Cubic Meter Per Second")] CubicMeterPerSecond,
        [Abbreviation("Pa")][Description("Pascal")] Pascal,
        [Abbreviation("kPa")][Description("Kilopascal")] Kilopascal,
        [Abbreviation("Ba")][Description("Bar")] Bar,
        [Abbreviation("psi")][Description("Pound Per Square Inch")] PoundPerSquareInch,
        [Abbreviation("kJ")][Description("Kilojule")] Kilojule,
        [Abbreviation("kJ/kg")][Description("Kilojule Per Kilogram")] KilojulePerKilogram,
        [Abbreviation("J/kg")][Description("Jule Per Kilogram")] JulePerKilogram,
        [Abbreviation("J")][Description("Jule")] Jule,
        [Abbreviation("W")][Description("Watt")] Watt,
        [Abbreviation("kW")][Description("Kliowatt")] Kilowatt,
        [Abbreviation("l/s")][Description("Liters Per Second")] LitersPerSecond,
        [Abbreviation("N/m2")][Description("Newton Per Squere Meter")] NewtonPerSquereMeter,
        [Abbreviation("g/g")][Description("Gram Per Gram")] GramPerGram,
        [Abbreviation("m2")][Description("Square Meter")] SquareMeter,
        [Abbreviation("ft2")][Description("Square Foot")] SquareFoot,
        [Abbreviation("m3")][Description("Cubic Meter")] CubicMeter,
        [Abbreviation("ft3")][Description("Cubic Foot")] CubicFoot,
        [Abbreviation("cfm")][Description("Cubic Foot Per Minute")] CubicFootPerMinute,
        [Abbreviation("Btu/h")][Description("Btu Per Hour")] BtuPerHour,
        [Abbreviation("kBtu/h")][Description("Kilo Btu Per Hour")] KiloBtuPerHour,
        [Abbreviation("W/m2")][Description("Watt Per Square Meter")] WattPerSquareMeter,
        [Abbreviation("Btu/h.ft2")][Description("Btu Per Hour Square Foot")] BtuPerHourSquareFoot,
        [Abbreviation("dK")][Description("Kelvin Difference")] KelvinDifference,
        [Abbreviation("dF")][Description("Fahrenheit Difference")] FahrenheitDifference,
        [Abbreviation("W/person")][Description("Watt Per Person")] WattPerPerson,
        [Abbreviation("Btu/h.person")][Description("Btu Per Hour Per Person")] BtuPerHourPerPerson,
        [Abbreviation("m2/person")][Description("Square Meter Per Person")] SquareMeterPerPerson,
        [Abbreviation("ft2/person")][Description("Square Foot Per Person")] SquareFootPerPerson,
        [Abbreviation("ac/h")][Description("Air Changes Per Hour")] AirChangesPerHour,
        [Abbreviation("lx")][Description("Lux")] Lux,
        [Abbreviation("fc")][Description("Foot Candle")] FootCandle,
        [Abbreviation("W/m2K")][Description("Watt Per Square Meter Kelvin")] WattPerSquareMeterKelvin,
        [Abbreviation("Btu/h.ft2.F")][Description("Btu Per Hour Square Foot Fahrenheit")] BtuPerHourSquareFootFahrenheit,
        [Abbreviation("person")][Description("Person")] Person,
        [Abbreviation("h")][Description("Hour")] Hour,
        [Abbreviation("deg")][Description("Degree")] Degree,
        [Abbreviation("rad")][Description("Radian")] Radian,
    }
}
