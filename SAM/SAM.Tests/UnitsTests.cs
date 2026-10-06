// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Units;
using Xunit;

namespace SAM.Tests
{
    public class UnitsTests
    {
        private static readonly UnitType[] allUnitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToArray();
        private static readonly UnitType[] definedUnitTypes = allUnitTypes.Where(x => x != UnitType.Undefined).ToArray();

        private static void AssertClose(double expected, double actual, double relativeTolerance = 1e-9)
        {
            Assert.False(double.IsNaN(actual), $"expected {expected}, got NaN");
            double tolerance = System.Math.Max(System.Math.Abs(expected), 1.0) * relativeTolerance;
            Assert.True(System.Math.Abs(expected - actual) <= tolerance, $"expected {expected}, got {actual}");
        }

        // ---------- Enum compatibility ----------

        [Fact]
        public void UnitType_ExistingOrdinals_AreUnchanged()
        {
            string[] expected =
            {
                "Undefined", "Meter", "Feet", "Celsius", "Kelvin", "Fahrenheit", "KilogramPerKilogram", "GramPerKilogram",
                "Percent", "Unitless", "KilogramPerCubicMeter", "CubicMeterPerKilogram", "CubicMeterPerGram",
                "CubicMeterPerHour", "CubicMeterPerSecond", "Pascal", "Kilopascal", "Bar", "PoundPerSquareInch", "Kilojule",
                "KilojulePerKilogram", "JulePerKilogram", "Jule", "Watt", "Kilowatt", "LitersPerSecond",
                "NewtonPerSquereMeter", "GramPerGram",
            };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], ((UnitType)i).ToString());
            }
        }

        [Fact]
        public void UnitCategory_ExistingOrdinals_AreUnchanged()
        {
            string[] expected =
            {
                "Undefined", "Temperature", "HumidityRatio", "Density", "SpecificVolume", "Pressure", "AirFlow",
                "RelativeHumidity", "Efficiency", "Enthaply", "SpecificEnthaply", "Power",
            };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], ((UnitCategory)i).ToString());
            }
        }

        // ---------- Reference values per family ----------

        [Theory]
        // Length
        [InlineData(UnitType.Meter, UnitType.Feet, 1.0, 3.280839895)]
        // Area / Volume
        [InlineData(UnitType.SquareMeter, UnitType.SquareFoot, 1.0, 10.7639104)]
        [InlineData(UnitType.CubicMeter, UnitType.CubicFoot, 1.0, 35.3146667)]
        // AirFlow
        [InlineData(UnitType.CubicMeterPerSecond, UnitType.LitersPerSecond, 1.0, 1000.0)]
        [InlineData(UnitType.CubicMeterPerSecond, UnitType.CubicMeterPerHour, 1.0, 3600.0)]
        [InlineData(UnitType.CubicMeterPerSecond, UnitType.CubicFootPerMinute, 1.0, 2118.880003)]
        [InlineData(UnitType.LitersPerSecond, UnitType.CubicFootPerMinute, 100.0, 211.8880003)]
        [InlineData(UnitType.LitersPerSecond, UnitType.CubicMeterPerHour, 100.0, 360.0)]
        // Power
        [InlineData(UnitType.Kilowatt, UnitType.Watt, 1.0, 1000.0)]
        [InlineData(UnitType.Watt, UnitType.BtuPerHour, 1.0, 3.412141633)]
        [InlineData(UnitType.Kilowatt, UnitType.KiloBtuPerHour, 1.0, 3.412141633)]
        [InlineData(UnitType.KiloBtuPerHour, UnitType.BtuPerHour, 1.0, 1000.0)]
        // SpecificPower
        [InlineData(UnitType.WattPerSquareMeter, UnitType.BtuPerHourSquareFoot, 1.0, 0.316998331)]
        // TemperatureDifference
        [InlineData(UnitType.KelvinDifference, UnitType.FahrenheitDifference, 10.0, 18.0)]
        // Per person
        [InlineData(UnitType.WattPerPerson, UnitType.BtuPerHourPerPerson, 1.0, 3.412141633)]
        [InlineData(UnitType.SquareMeterPerPerson, UnitType.SquareFootPerPerson, 1.0, 10.7639104)]
        // Illuminance
        [InlineData(UnitType.Lux, UnitType.FootCandle, 1.0, 0.09290304)]
        // ThermalTransmittance
        [InlineData(UnitType.WattPerSquareMeterKelvin, UnitType.BtuPerHourSquareFootFahrenheit, 1.0, 0.176110184)]
        // Ratio
        [InlineData(UnitType.Percent, UnitType.Unitless, 50.0, 0.5)]
        // Angle
        [InlineData(UnitType.Degree, UnitType.Radian, 180.0, System.Math.PI)]
        // Pressure
        [InlineData(UnitType.Bar, UnitType.Pascal, 1.0, 100000.0)]
        [InlineData(UnitType.PoundPerSquareInch, UnitType.Pascal, 1.0, 6894.75728)]
        [InlineData(UnitType.NewtonPerSquereMeter, UnitType.Pascal, 1.0, 1.0)]
        // Energy / specific enthalpy / specific volume
        [InlineData(UnitType.Kilojule, UnitType.Jule, 1.0, 1000.0)]
        [InlineData(UnitType.KilojulePerKilogram, UnitType.JulePerKilogram, 1.0, 1000.0)]
        [InlineData(UnitType.CubicMeterPerGram, UnitType.CubicMeterPerKilogram, 1.0, 1000.0)]
        // HumidityRatio
        [InlineData(UnitType.KilogramPerKilogram, UnitType.GramPerKilogram, 0.008, 8.0)]
        [InlineData(UnitType.GramPerGram, UnitType.KilogramPerKilogram, 0.008, 0.008)]
        public void ByUnitType_ReferenceValues(UnitType from, UnitType to, double value, double expected)
        {
            AssertClose(expected, Units.Convert.ByUnitType(value, from, to), 1e-8);
            AssertClose(value, Units.Convert.ByUnitType(expected, to, from), 1e-8);
        }

        [Fact]
        public void Factor_DerivedConstants_AreMutuallyConsistent()
        {
            // W/m2 -> Btu/h.ft2 is W -> Btu/h over m2 -> ft2, and the U-value factor is that over the 1.8 K -> F step.
            AssertClose(Factor.WattsToBtuPerHour / Factor.SquareMetersToSquareFeet, Factor.WattsPerSquareMeterToBtuPerHourSquareFoot, 1e-8);
            AssertClose(Factor.WattsPerSquareMeterToBtuPerHourSquareFoot / Factor.KelvinDifferenceToFahrenheitDifference, Factor.WattsPerSquareMeterKelvinToBtuPerHourSquareFootFahrenheit, 1e-8);
            AssertClose(1 / Factor.SquareMetersToSquareFeet, Factor.LuxToFootCandles, 1e-8);
            AssertClose(Factor.CubicMetersToCubicFeet * 60, Factor.CubicMetersPerSecondToCubicFeetPerMinute, 1e-8);
        }

        // ---------- Round trips and family closure ----------

        [Fact]
        public void ByUnitType_EveryConvertiblePair_RoundTrips()
        {
            int count = 0;
            foreach (UnitType from in definedUnitTypes)
            {
                foreach (UnitType to in definedUnitTypes)
                {
                    foreach (double value in new[] { -40.0, 0.0, 1.0, 123.456 })
                    {
                        double converted = Units.Convert.ByUnitType(value, from, to);
                        if (double.IsNaN(converted))
                        {
                            continue;
                        }

                        AssertClose(value, Units.Convert.ByUnitType(converted, to, from), 1e-9);
                        count++;
                    }
                }
            }

            Assert.True(count > 0);
        }

        [Fact]
        public void ByUnitType_SameCategory_IsConvertible_CrossCategory_IsNaN()
        {
            foreach (UnitType from in definedUnitTypes)
            {
                foreach (UnitType to in definedUnitTypes)
                {
                    double converted = Units.Convert.ByUnitType(1.0, from, to);
                    if (from.UnitCategory() == to.UnitCategory())
                    {
                        Assert.False(double.IsNaN(converted), $"{from} -> {to} should be convertible");
                    }
                    else
                    {
                        Assert.True(double.IsNaN(converted), $"{from} -> {to} crosses categories and should be NaN");
                    }
                }
            }
        }

        // ---------- Temperatures ----------

        [Theory]
        [InlineData(-40.0, -40.0)]
        [InlineData(0.0, 32.0)]
        [InlineData(100.0, 212.0)]
        [InlineData(21.0, 69.8)]
        public void Temperature_CelsiusFahrenheit(double celsius, double fahrenheit)
        {
            AssertClose(fahrenheit, Units.Convert.ByUnitType(celsius, UnitType.Celsius, UnitType.Fahrenheit));
            AssertClose(celsius, Units.Convert.ByUnitType(fahrenheit, UnitType.Fahrenheit, UnitType.Celsius));
        }

        [Fact]
        public void Temperature_FahrenheitToCelsius_DividesBy1Point8_Regression()
        {
            // The old code divided by 18: 50 F gave 1 C instead of 10 C.
            AssertClose(10.0, Units.Convert.ByUnitType(50.0, UnitType.Fahrenheit, UnitType.Celsius));
            // Fahrenheit -> Kelvin goes through Celsius, so it was wrong too.
            AssertClose(273.15, Units.Convert.ByUnitType(32.0, UnitType.Fahrenheit, UnitType.Kelvin));
            AssertClose(233.15, Units.Convert.ByUnitType(-40.0, UnitType.Fahrenheit, UnitType.Kelvin));
            AssertClose(273.15, Units.Convert.ToSI(32.0, UnitType.Fahrenheit));
        }

        [Fact]
        public void Temperature_Kelvin()
        {
            AssertClose(273.15, Units.Convert.ByUnitType(0.0, UnitType.Celsius, UnitType.Kelvin));
            AssertClose(-273.15, Units.Convert.ByUnitType(0.0, UnitType.Kelvin, UnitType.Celsius));
            AssertClose(32.0, Units.Convert.ByUnitType(273.15, UnitType.Kelvin, UnitType.Fahrenheit));
        }

        [Fact]
        public void TemperatureDifference_HasNoOffset()
        {
            Assert.Equal(0.0, Units.Convert.ByUnitType(0.0, UnitType.KelvinDifference, UnitType.FahrenheitDifference));
            Assert.Equal(0.0, Units.Convert.ByUnitType(0.0, UnitType.FahrenheitDifference, UnitType.KelvinDifference));
            AssertClose(-9.0, Units.Convert.ByUnitType(-5.0, UnitType.KelvinDifference, UnitType.FahrenheitDifference));
            AssertClose(5.0, Units.Convert.ByUnitType(9.0, UnitType.FahrenheitDifference, UnitType.KelvinDifference));

            // An absolute temperature is not a difference: the two categories do not convert into each other.
            Assert.True(double.IsNaN(Units.Convert.ByUnitType(1.0, UnitType.Kelvin, UnitType.KelvinDifference)));
            Assert.True(double.IsNaN(Units.Convert.ByUnitType(1.0, UnitType.Fahrenheit, UnitType.FahrenheitDifference)));
        }

        // ---------- Humidity ratio regressions ----------

        [Fact]
        public void HumidityRatio_GramPerGram_Regressions()
        {
            // 1 g/g = 1000 g/kg (the old code divided by 1000, and multiplied the reverse way).
            AssertClose(8.0, Units.Convert.ByUnitType(0.008, UnitType.GramPerGram, UnitType.GramPerKilogram));
            AssertClose(0.008, Units.Convert.ByUnitType(8.0, UnitType.GramPerKilogram, UnitType.GramPerGram));
            AssertClose(0.008, Units.Convert.ByUnitType(0.008, UnitType.KilogramPerKilogram, UnitType.GramPerGram));
            AssertClose(0.008, Units.Convert.ByUnitType(0.008, UnitType.GramPerGram, UnitType.KilogramPerKilogram));
            AssertClose(0.008, Units.Convert.ByUnitType(8.0, UnitType.GramPerKilogram, UnitType.KilogramPerKilogram));
        }

        // ---------- ToSI / ToImperial ----------

        [Fact]
        public void ToSI_ExistingMappings_AreUnchanged()
        {
            AssertClose(1.0 / Factor.MetersToFeet, Units.Convert.ToSI(1.0, UnitType.Feet));
            AssertClose(293.15, Units.Convert.ToSI(20.0, UnitType.Celsius));
            AssertClose(0.008, Units.Convert.ToSI(8.0, UnitType.GramPerKilogram));
            AssertClose(0.5, Units.Convert.ToSI(50.0, UnitType.Percent));
            AssertClose(1.0, Units.Convert.ToSI(3600.0, UnitType.CubicMeterPerHour));
            AssertClose(100000.0, Units.Convert.ToSI(1.0, UnitType.Bar));
            AssertClose(1000.0, Units.Convert.ToSI(1.0, UnitType.Kilojule));
            Assert.True(double.IsNaN(Units.Convert.ToSI(1.0, UnitType.Undefined)));
        }

        [Theory]
        [InlineData(UnitType.SquareFoot, 10.7639104, 1.0)]
        [InlineData(UnitType.CubicFoot, 35.3146667, 1.0)]
        [InlineData(UnitType.LitersPerSecond, 1000.0, 1.0)]
        [InlineData(UnitType.CubicFootPerMinute, 2118.880003, 1.0)]
        [InlineData(UnitType.Kilowatt, 1.0, 1000.0)]
        [InlineData(UnitType.KiloBtuPerHour, 3.412141633, 1000.0)]
        [InlineData(UnitType.BtuPerHourSquareFoot, 0.316998331, 1.0)]
        [InlineData(UnitType.FahrenheitDifference, 18.0, 10.0)]
        [InlineData(UnitType.FootCandle, 0.09290304, 1.0)]
        [InlineData(UnitType.BtuPerHourSquareFootFahrenheit, 0.176110184, 1.0)]
        [InlineData(UnitType.Degree, 180.0, System.Math.PI)]
        [InlineData(UnitType.AirChangesPerHour, 2.0, 2.0)]
        public void ToSI_NewTypes(UnitType from, double value, double expected)
        {
            AssertClose(expected, Units.Convert.ToSI(value, from), 1e-8);
        }

        [Theory]
        [InlineData(UnitType.Meter, 1.0, 3.280839895)]
        [InlineData(UnitType.Celsius, 0.0, 32.0)]
        [InlineData(UnitType.SquareMeter, 1.0, 10.7639104)]
        [InlineData(UnitType.CubicMeter, 1.0, 35.3146667)]
        [InlineData(UnitType.CubicMeterPerSecond, 1.0, 2118.880003)]
        [InlineData(UnitType.LitersPerSecond, 1000.0, 2118.880003)]
        [InlineData(UnitType.Kilowatt, 1.0, 3412.141633)]
        [InlineData(UnitType.KiloBtuPerHour, 1.0, 1000.0)]
        [InlineData(UnitType.WattPerSquareMeter, 1.0, 0.316998331)]
        [InlineData(UnitType.KelvinDifference, 10.0, 18.0)]
        [InlineData(UnitType.Lux, 1.0, 0.09290304)]
        [InlineData(UnitType.WattPerSquareMeterKelvin, 1.0, 0.176110184)]
        [InlineData(UnitType.Radian, System.Math.PI, 180.0)]
        public void ToImperial_NewTypes(UnitType from, double value, double expected)
        {
            AssertClose(expected, Units.Convert.ToImperial(value, from), 1e-8);
        }

        [Fact]
        public void ToSI_ToImperial_AreDefinedForEveryNewType()
        {
            foreach (UnitType unitType in definedUnitTypes.Where(x => (int)x > (int)UnitType.GramPerGram))
            {
                Assert.False(double.IsNaN(Units.Convert.ToSI(1.0, unitType)), $"ToSI({unitType})");
                Assert.False(double.IsNaN(Units.Convert.ToImperial(1.0, unitType)), $"ToImperial({unitType})");
            }
        }

        // ---------- Abbreviations and parsing ----------

        [Fact]
        public void Abbreviation_IsUniqueAndPresent_ForEveryUnitType()
        {
            foreach (UnitType unitType in definedUnitTypes)
            {
                Assert.False(string.IsNullOrWhiteSpace(unitType.Abbreviation()), $"{unitType} has no abbreviation");
                Assert.False(string.IsNullOrWhiteSpace(unitType.Description()), $"{unitType} has no description");
            }

            List<string> duplicates = definedUnitTypes
                .GroupBy(x => x.Abbreviation().ToUpperInvariant())
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            Assert.Empty(duplicates);
        }

        [Fact]
        public void UnitType_ParsesEveryAbbreviationAndDescription_BackToItsOwnUnit()
        {
            foreach (UnitType unitType in definedUnitTypes)
            {
                string abbreviation = unitType.Abbreviation();
                string description = unitType.Description();

                Assert.Equal(unitType, Units.Query.UnitType(unitType.ToString()));
                Assert.Equal(unitType, Units.Query.UnitType(abbreviation));
                Assert.Equal(unitType, Units.Query.UnitType(abbreviation.ToUpperInvariant()));
                Assert.Equal(unitType, Units.Query.UnitType(abbreviation.ToLowerInvariant()));
                Assert.Equal(unitType, Units.Query.UnitType(description));
                Assert.Equal(unitType, Units.Query.UnitType(description.ToUpperInvariant()));
            }
        }

        [Theory]
        [InlineData("METER", UnitType.Meter)]      // old parser returned Celsius
        [InlineData("FEET", UnitType.Feet)]        // old parser returned Fahrenheit
        [InlineData("KG/KG", UnitType.KilogramPerKilogram)] // old parser returned CubicMeterPerHour
        [InlineData("L/S", UnitType.LitersPerSecond)]
        [InlineData("cubic meter per second", UnitType.CubicMeterPerSecond)]
        [InlineData("UNDEFINED", UnitType.Undefined)]
        [InlineData("no such unit", UnitType.Undefined)]
        [InlineData("", UnitType.Undefined)]
        public void UnitType_CaseInsensitiveParser_Regression(string text, UnitType expected)
        {
            Assert.Equal(expected, Units.Query.UnitType(text));
        }

        // ---------- Categories ----------

        [Fact]
        public void UnitCategory_IsDefined_ForEveryUnitType()
        {
            Assert.Equal(UnitCategory.Undefined, UnitType.Undefined.UnitCategory());

            foreach (UnitType unitType in definedUnitTypes)
            {
                Assert.NotEqual(UnitCategory.Undefined, unitType.UnitCategory());
            }
        }

        [Theory]
        [InlineData(UnitType.Unitless, UnitCategory.Ratio)]
        [InlineData(UnitType.Percent, UnitCategory.Ratio)]
        [InlineData(UnitType.CubicMeterPerSecond, UnitCategory.AirFlow)]
        [InlineData(UnitType.LitersPerSecond, UnitCategory.AirFlow)]
        [InlineData(UnitType.Kelvin, UnitCategory.Temperature)]
        [InlineData(UnitType.KelvinDifference, UnitCategory.TemperatureDifference)]
        [InlineData(UnitType.Meter, UnitCategory.Length)]
        [InlineData(UnitType.Person, UnitCategory.Count)]
        public void UnitCategory_PrimaryCategory(UnitType unitType, UnitCategory expected)
        {
            Assert.Equal(expected, unitType.UnitCategory());
        }

        [Theory]
        // Compatibility lock: the pre-change SI results, which the Mollier UI depends on.
        [InlineData(UnitCategory.Temperature, UnitType.Celsius)]
        [InlineData(UnitCategory.HumidityRatio, UnitType.KilogramPerKilogram)]
        [InlineData(UnitCategory.Density, UnitType.KilogramPerCubicMeter)]
        [InlineData(UnitCategory.SpecificVolume, UnitType.CubicMeterPerKilogram)]
        [InlineData(UnitCategory.Pressure, UnitType.Pascal)]
        [InlineData(UnitCategory.AirFlow, UnitType.CubicMeterPerSecond)]
        [InlineData(UnitCategory.RelativeHumidity, UnitType.Unitless)]
        [InlineData(UnitCategory.Efficiency, UnitType.Unitless)]
        [InlineData(UnitCategory.Enthaply, UnitType.Jule)]
        [InlineData(UnitCategory.SpecificEnthaply, UnitType.JulePerKilogram)]
        [InlineData(UnitCategory.Power, UnitType.Watt)]
        [InlineData(UnitCategory.Undefined, UnitType.Undefined)]
        public void UnitType_SI_ExistingCategories_AreUnchanged(UnitCategory unitCategory, UnitType expected)
        {
            Assert.Equal(expected, UnitStyle.SI.UnitType(unitCategory));
        }

        [Theory]
        [InlineData(UnitCategory.Pressure, UnitType.PoundPerSquareInch)]
        [InlineData(UnitCategory.Temperature, UnitType.Fahrenheit)]
        [InlineData(UnitCategory.AirFlow, UnitType.CubicFootPerMinute)]
        [InlineData(UnitCategory.Power, UnitType.BtuPerHour)]
        public void UnitType_Imperial_ExistingCategories(UnitCategory unitCategory, UnitType expected)
        {
            Assert.Equal(expected, UnitStyle.Imperial.UnitType(unitCategory));
        }

        [Fact]
        public void UnitType_IsValid_ForEveryNewCategory_InBothStyles()
        {
            IEnumerable<UnitCategory> newCategories = Enum.GetValues(typeof(UnitCategory)).Cast<UnitCategory>()
                .Where(x => (int)x > (int)UnitCategory.Power);

            foreach (UnitCategory unitCategory in newCategories)
            {
                foreach (UnitStyle unitStyle in new[] { UnitStyle.SI, UnitStyle.Imperial })
                {
                    UnitType unitType = unitStyle.UnitType(unitCategory);
                    Assert.NotEqual(UnitType.Undefined, unitType);
                    Assert.Equal(unitCategory, unitType.UnitCategory());

                    List<UnitType> unitTypes = unitCategory.UnitTypes(unitStyle);
                    Assert.NotNull(unitTypes);
                    Assert.Contains(unitType, unitTypes);
                    Assert.All(unitTypes, x => Assert.Equal(unitCategory, x.UnitCategory()));
                }

                Assert.NotEmpty(unitCategory.UnitTypes());
            }
        }

        [Fact]
        public void UnitTypes_ImperialEntries_ForExistingCategories()
        {
            Assert.Equal(new[] { UnitType.CubicFootPerMinute }, UnitCategory.AirFlow.UnitTypes(UnitStyle.Imperial));
            Assert.Equal(new[] { UnitType.BtuPerHour, UnitType.KiloBtuPerHour }, UnitCategory.Power.UnitTypes(UnitStyle.Imperial));
            Assert.Equal(new[] { UnitType.Fahrenheit }, UnitCategory.Temperature.UnitTypes(UnitStyle.Imperial));
        }

        // ---------- Quantity ----------

        [Fact]
        public void Quantity_ConvertTo()
        {
            Quantity airFlow = new Quantity(0.198, UnitType.CubicMeterPerSecond);

            Quantity litersPerSecond = airFlow.ConvertTo(UnitType.LitersPerSecond);
            Assert.Equal(UnitType.LitersPerSecond, litersPerSecond.Unit);
            AssertClose(198.0, litersPerSecond.Value);
            Assert.Equal(UnitCategory.AirFlow, litersPerSecond.Category);
            Assert.True(litersPerSecond.IsValid);

            // The original is unchanged (immutable value).
            Assert.Equal(0.198, airFlow.Value);
            Assert.Equal(UnitType.CubicMeterPerSecond, airFlow.Unit);

            Quantity power = new Quantity(12400, UnitType.Watt).ConvertTo(UnitType.Kilowatt);
            AssertClose(12.4, power.Value);

            Quantity temperature = new Quantity(-40, UnitType.Celsius).ConvertTo(UnitType.Fahrenheit);
            AssertClose(-40.0, temperature.Value);
        }

        [Fact]
        public void Quantity_UnsupportedConversion_IsNaNAndInvalid()
        {
            Quantity quantity = new Quantity(39.3, UnitType.SquareMeter).ConvertTo(UnitType.Watt);

            Assert.True(double.IsNaN(quantity.Value));
            Assert.Equal(UnitType.Watt, quantity.Unit);
            Assert.False(quantity.IsValid);
        }

        [Fact]
        public void Quantity_IsValid()
        {
            Assert.True(new Quantity(0, UnitType.Watt).IsValid);
            Assert.False(new Quantity(double.NaN, UnitType.Watt).IsValid);
            Assert.False(new Quantity(double.PositiveInfinity, UnitType.Watt).IsValid);
            Assert.False(new Quantity(1, UnitType.Undefined).IsValid);
            Assert.False(default(Quantity).IsValid);
        }

        [Fact]
        public void Quantity_ToString_UsesInvariantCultureAndAbbreviation()
        {
            System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
                Assert.Equal("39.3 m2", new Quantity(39.3, UnitType.SquareMeter).ToString());
                Assert.Equal("2.5", new Quantity(2.5, UnitType.Undefined).ToString());
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = cultureInfo;
            }
        }
    }
}
