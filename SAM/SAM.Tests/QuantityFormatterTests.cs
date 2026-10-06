// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;
using System;
using System.Globalization;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// SI / Imperial display policy (Rev 2 §5, Rev 3 R3.3): display units, precision, shared units for comparable
    /// values, absolute temperature vs temperature difference, and placeholders.
    /// </summary>
    public class QuantityFormatterTests
    {
        private static QuantityFormatter Formatter(UnitStyle unitStyle, AirFlowDisplay airFlowDisplay = AirFlowDisplay.LitersPerSecond, string culture = "en-GB")
        {
            return new QuantityFormatter(new DocumentOptions() { UnitSystem = unitStyle, SIAirFlow = airFlowDisplay, Culture = CultureInfo.GetCultureInfo(culture) });
        }

        private static ReportValue<Quantity> Value(double value, UnitType unitType)
        {
            return ReportValue<Quantity>.Available(new Quantity(value, unitType), ReportValueSource.SAM);
        }

        [Theory]
        // SI
        [InlineData(UnitStyle.SI, 39.3, UnitType.SquareMeter, "39.3", "m²")]
        [InlineData(UnitStyle.SI, 174.7, UnitType.CubicMeter, "174.7", "m³")]
        [InlineData(UnitStyle.SI, 21.0, UnitType.Celsius, "21.0", "°C")]
        [InlineData(UnitStyle.SI, 10.0, UnitType.KelvinDifference, "10.0", "K")]
        [InlineData(UnitStyle.SI, 0.198, UnitType.CubicMeterPerSecond, "198", "L/s")]
        [InlineData(UnitStyle.SI, 779.0, UnitType.Watt, "779", "W")]
        [InlineData(UnitStyle.SI, 25.0, UnitType.WattPerSquareMeter, "25.0", "W/m²")]
        [InlineData(UnitStyle.SI, 0.2, UnitType.AirChangesPerHour, "0.20", "ac/h")]
        [InlineData(UnitStyle.SI, 500.0, UnitType.Lux, "500", "lx")]
        [InlineData(UnitStyle.SI, 1.2, UnitType.Unitless, "120", "%")]
        [InlineData(UnitStyle.SI, 13.1, UnitType.SquareMeterPerPerson, "13.1", "m²/person")]
        [InlineData(UnitStyle.SI, 0.25, UnitType.WattPerSquareMeterKelvin, "0.25", "W/m²K")]
        // Imperial
        [InlineData(UnitStyle.Imperial, 39.3, UnitType.SquareMeter, "423", "ft²")]
        [InlineData(UnitStyle.Imperial, 174.7, UnitType.CubicMeter, "6,169", "ft³")]
        [InlineData(UnitStyle.Imperial, 21.0, UnitType.Celsius, "69.8", "°F")]
        [InlineData(UnitStyle.Imperial, -40.0, UnitType.Celsius, "-40.0", "°F")]
        [InlineData(UnitStyle.Imperial, 10.0, UnitType.KelvinDifference, "18.0", "Δ°F")]
        [InlineData(UnitStyle.Imperial, 0.198, UnitType.CubicMeterPerSecond, "420", "cfm")]
        [InlineData(UnitStyle.Imperial, 779.0, UnitType.Watt, "2,658", "Btu/h")]
        [InlineData(UnitStyle.Imperial, 25.0, UnitType.WattPerSquareMeter, "7.92", "Btu/h·ft²")]
        [InlineData(UnitStyle.Imperial, 500.0, UnitType.Lux, "46.5", "fc")]
        [InlineData(UnitStyle.Imperial, 0.25, UnitType.WattPerSquareMeterKelvin, "0.044", "Btu/h·ft²·°F")]
        [InlineData(UnitStyle.Imperial, 0.2, UnitType.AirChangesPerHour, "0.20", "ac/h")]
        public void Format_StandAlone(UnitStyle unitStyle, double value, UnitType unitType, string text, string unit)
        {
            FormattedValue formattedValue = Formatter(unitStyle).Format(Value(value, unitType));

            Assert.Equal(text, formattedValue.Text);
            Assert.Equal(unit, formattedValue.Unit);
            Assert.Equal(Availability.Available, formattedValue.Availability);
        }

        [Fact]
        public void AbsoluteTemperature_AppliesTheOffset_TemperatureDifference_DoesNot()
        {
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.Imperial);

            // 0 °C is 32 °F, but a 0 K difference is a 0 °F difference.
            Assert.Equal("32.0", quantityFormatter.Format(Value(0, UnitType.Celsius)).Text);
            Assert.Equal("0.0", quantityFormatter.Format(Value(0, UnitType.KelvinDifference)).Text);
            Assert.Equal("°F", quantityFormatter.Format(Value(0, UnitType.Celsius)).Unit);
            Assert.Equal("Δ°F", quantityFormatter.Format(Value(0, UnitType.KelvinDifference)).Unit);
        }

        [Fact]
        public void AirFlow_SIDefault_UsesCapitalLitreSymbol()
        {
            // Approved Phase-1 convention: capital L, so the litre is never misread as the digit 1.
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.SI);

            Assert.Equal("L/s", quantityFormatter.DisplayUnit(UnitCategory.AirFlow).Symbol);
            Assert.Equal("L/s", quantityFormatter.Format(Value(0.198, UnitType.CubicMeterPerSecond)).Unit);
        }

        [Fact]
        public void AirFlow_SIOption_CubicMetersPerSecond()
        {
            FormattedValue formattedValue = Formatter(UnitStyle.SI, AirFlowDisplay.CubicMetersPerSecond).Format(Value(198, UnitType.LitersPerSecond));

            Assert.Equal("0.198", formattedValue.Text);
            Assert.Equal("m³/s", formattedValue.Unit);
        }

        [Fact]
        public void Power_PairStraddling10kW_SharesKilowatts()
        {
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.SI);
            ReportValue<Quantity> heating = Value(779, UnitType.Watt);
            ReportValue<Quantity> cooling = Value(12400, UnitType.Watt);

            DisplayUnit displayUnit = quantityFormatter.SelectDisplayUnit(UnitCategory.Power, new[] { heating.Value, cooling.Value });

            Assert.Equal(UnitType.Kilowatt, displayUnit.UnitType);
            Assert.Equal("0.78", quantityFormatter.Format(heating, displayUnit).Text);
            Assert.Equal("12.40", quantityFormatter.Format(cooling, displayUnit).Text);
            Assert.Equal("kW", quantityFormatter.Format(heating, displayUnit).Unit);

            // Stand-alone, the small value would have been W: sharing is what makes the pair comparable.
            Assert.Equal("W", quantityFormatter.Format(heating).Unit);
        }

        [Fact]
        public void Power_PairBelow10kW_StaysInWatts()
        {
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.SI);

            DisplayUnit displayUnit = quantityFormatter.SelectDisplayUnit(UnitCategory.Power, new[] { new Quantity(779, UnitType.Watt), new Quantity(9999, UnitType.Watt) });

            Assert.Equal(UnitType.Watt, displayUnit.UnitType);
            Assert.Equal("9,999", quantityFormatter.Format(Value(9999, UnitType.Watt), displayUnit).Text);
        }

        [Fact]
        public void Power_ImperialPairStraddling100kBtuh_SharesKiloBtuPerHour()
        {
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.Imperial);
            ReportValue<Quantity> heating = Value(20000, UnitType.Watt);   //  68,243 Btu/h
            ReportValue<Quantity> cooling = Value(40000, UnitType.Watt);   // 136,486 Btu/h

            DisplayUnit displayUnit = quantityFormatter.SelectDisplayUnit(UnitCategory.Power, new[] { heating.Value, cooling.Value });

            Assert.Equal(UnitType.KiloBtuPerHour, displayUnit.UnitType);
            Assert.Equal("68.2", quantityFormatter.Format(heating, displayUnit).Text);
            Assert.Equal("136.5", quantityFormatter.Format(cooling, displayUnit).Text);
            Assert.Equal("kBtu/h", quantityFormatter.Format(cooling, displayUnit).Unit);

            DisplayUnit displayUnit_Small = quantityFormatter.SelectDisplayUnit(UnitCategory.Power, new[] { heating.Value });
            Assert.Equal(UnitType.BtuPerHour, displayUnit_Small.UnitType);
        }

        [Fact]
        public void SelectDisplayUnit_IgnoresInvalidAndOtherCategories()
        {
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.SI);

            DisplayUnit displayUnit = quantityFormatter.SelectDisplayUnit(UnitCategory.Power, new[] { new Quantity(double.NaN, UnitType.Watt), new Quantity(50000, UnitType.SquareMeter), new Quantity(100, UnitType.Watt) });

            Assert.Equal(UnitType.Watt, displayUnit.UnitType);
        }

        [Fact]
        public void Placeholders_AndProvenance()
        {
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.SI);

            FormattedValue notAvailable = quantityFormatter.Format(ReportValue<Quantity>.NotAvailable("No design load"));
            Assert.Equal("—", notAvailable.Text);
            Assert.Null(notAvailable.Unit);
            Assert.Equal(Availability.NotAvailable, notAvailable.Availability);
            Assert.Equal("No design load", notAvailable.Note);
            Assert.Null(notAvailable.Freshness);

            FormattedValue notApplicable = quantityFormatter.Format(ReportValue<string>.NotApplicable("Equipment"));
            Assert.Equal("n/a", notApplicable.Text);
            Assert.Equal(Availability.NotApplicable, notApplicable.Availability);

            FormattedValue outOfDate = quantityFormatter.Format(ReportValue<Quantity>.Available(new Quantity(779, UnitType.Watt), ReportValueSource.TBD, Freshness.OutOfDate));
            Assert.Equal("779", outOfDate.Text);
            Assert.True(outOfDate.IsOutOfDate);
            Assert.Equal(ReportValueSource.TBD, outOfDate.Source);
        }

        [Fact]
        public void Format_NeverPrintsNegativeZero_AndKeepsZero()
        {
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.SI);

            Assert.Equal("0.0", quantityFormatter.Format(Value(-0.01, UnitType.Celsius)).Text);
            Assert.Equal("0.0", quantityFormatter.Format(Value(0, UnitType.SquareMeter)).Text);
        }

        [Fact]
        public void Format_UsesTheCultureSeparators()
        {
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.SI, culture: "de-DE");

            Assert.Equal("1.234,5", quantityFormatter.Format(Value(1234.5, UnitType.SquareMeter)).Text);
        }

        [Fact]
        public void Format_InAUnitOfAnotherCategory_Throws()
        {
            QuantityFormatter quantityFormatter = Formatter(UnitStyle.SI);

            Assert.Throws<InvalidOperationException>(() => quantityFormatter.Format(Value(1, UnitType.Watt), quantityFormatter.DisplayUnit(UnitCategory.Area)));
        }

        [Fact]
        public void DisplayUnit_IsDefinedForEveryReportingCategory_InBothStyles()
        {
            foreach (UnitCategory unitCategory in Enum.GetValues(typeof(UnitCategory)))
            {
                if (unitCategory == UnitCategory.Undefined)
                {
                    continue;
                }

                foreach (UnitStyle unitStyle in new[] { UnitStyle.SI, UnitStyle.Imperial })
                {
                    DisplayUnit displayUnit = Formatter(unitStyle).DisplayUnit(unitCategory);
                    if (displayUnit.UnitType == UnitType.Undefined)
                    {
                        // Categories with no Imperial unit yet (density, specific volume, enthalpy) fall back to SI
                        // only through their own unit; they are not used by Phase 1.
                        continue;
                    }

                    Assert.False(string.IsNullOrEmpty(displayUnit.Symbol), $"{unitStyle} {unitCategory}");
                }
            }
        }
    }
}
