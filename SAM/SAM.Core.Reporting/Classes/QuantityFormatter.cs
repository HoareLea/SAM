// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Units;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Default <see cref="IQuantityFormatter"/>: a display policy per <see cref="UnitCategory"/> and
    /// <see cref="UnitStyle"/>. Conversions go through <see cref="Quantity.ConvertTo(UnitType)"/>.
    /// </summary>
    public sealed class QuantityFormatter : IQuantityFormatter
    {
        /// <summary>
        /// SI power switches from W to kW when the group's largest value reaches 10 kW.
        /// </summary>
        public const double KilowattThreshold_W = 10000;

        /// <summary>
        /// Imperial power switches from Btu/h to kBtu/h when the group's largest value reaches 100 kBtu/h.
        /// </summary>
        public const double KiloBtuPerHourThreshold_BtuPerHour = 100000;

        private readonly CultureInfo cultureInfo;
        private readonly AirFlowDisplay airFlowDisplay;

        public QuantityFormatter(DocumentOptions documentOptions = null)
        {
            documentOptions = documentOptions ?? new DocumentOptions();

            UnitSystem = documentOptions.UnitSystem == UnitStyle.Imperial ? UnitStyle.Imperial : UnitStyle.SI;
            cultureInfo = documentOptions.Culture ?? CultureInfo.InvariantCulture;
            airFlowDisplay = documentOptions.SIAirFlow;
        }

        public UnitStyle UnitSystem { get; }

        public string NotAvailableText => "—";

        public string NotApplicableText => "n/a";

        public DisplayUnit DisplayUnit(UnitCategory unitCategory)
        {
            bool si = UnitSystem == UnitStyle.SI;

            switch (unitCategory)
            {
                case UnitCategory.Length:
                    return si ? new DisplayUnit(UnitType.Meter, "m", 2) : new DisplayUnit(UnitType.Feet, "ft", 1);

                case UnitCategory.Area:
                    return si ? new DisplayUnit(UnitType.SquareMeter, "m²", 1) : new DisplayUnit(UnitType.SquareFoot, "ft²", 0);

                case UnitCategory.Volume:
                    return si ? new DisplayUnit(UnitType.CubicMeter, "m³", 1) : new DisplayUnit(UnitType.CubicFoot, "ft³", 0);

                case UnitCategory.Temperature:
                    return si ? new DisplayUnit(UnitType.Celsius, "°C", 1) : new DisplayUnit(UnitType.Fahrenheit, "°F", 1);

                case UnitCategory.TemperatureDifference:
                    return si ? new DisplayUnit(UnitType.KelvinDifference, "K", 1) : new DisplayUnit(UnitType.FahrenheitDifference, "Δ°F", 1);

                case UnitCategory.AirFlow:
                    if (!si)
                    {
                        return new DisplayUnit(UnitType.CubicFootPerMinute, "cfm", 0);
                    }

                    return airFlowDisplay == AirFlowDisplay.CubicMetersPerSecond ? new DisplayUnit(UnitType.CubicMeterPerSecond, "m³/s", 3) : new DisplayUnit(UnitType.LitersPerSecond, "L/s", 0);

                case UnitCategory.Power:
                    return si ? new DisplayUnit(UnitType.Watt, "W", 0) : new DisplayUnit(UnitType.BtuPerHour, "Btu/h", 0);

                case UnitCategory.SpecificPower:
                    return si ? new DisplayUnit(UnitType.WattPerSquareMeter, "W/m²", 1) : new DisplayUnit(UnitType.BtuPerHourSquareFoot, "Btu/h·ft²", 2);

                case UnitCategory.PowerPerPerson:
                    return si ? new DisplayUnit(UnitType.WattPerPerson, "W/person", 0) : new DisplayUnit(UnitType.BtuPerHourPerPerson, "Btu/h/person", 0);

                case UnitCategory.AreaPerPerson:
                    return si ? new DisplayUnit(UnitType.SquareMeterPerPerson, "m²/person", 1) : new DisplayUnit(UnitType.SquareFootPerPerson, "ft²/person", 0);

                case UnitCategory.AirChangeRate:
                    return new DisplayUnit(UnitType.AirChangesPerHour, "ac/h", 2);

                case UnitCategory.RelativeHumidity:
                case UnitCategory.Ratio:
                case UnitCategory.Efficiency:
                    return new DisplayUnit(UnitType.Percent, "%", 0);

                case UnitCategory.HumidityRatio:
                    return new DisplayUnit(UnitType.GramPerKilogram, "g/kg", 1);

                case UnitCategory.Illuminance:
                    return si ? new DisplayUnit(UnitType.Lux, "lx", 0) : new DisplayUnit(UnitType.FootCandle, "fc", 1);

                case UnitCategory.ThermalTransmittance:
                    return si ? new DisplayUnit(UnitType.WattPerSquareMeterKelvin, "W/m²K", 2) : new DisplayUnit(UnitType.BtuPerHourSquareFootFahrenheit, "Btu/h·ft²·°F", 3);

                case UnitCategory.Count:
                    return new DisplayUnit(UnitType.Person, "persons", 1);

                case UnitCategory.Time:
                    return new DisplayUnit(UnitType.Hour, "h", 0);

                case UnitCategory.Angle:
                    return new DisplayUnit(UnitType.Degree, "°", 1);

                case UnitCategory.Pressure:
                    return si ? new DisplayUnit(UnitType.Pascal, "Pa", 0) : new DisplayUnit(UnitType.PoundPerSquareInch, "psi", 3);
            }

            UnitType unitType = UnitSystem.UnitType(unitCategory);
            return new DisplayUnit(unitType, unitType == UnitType.Undefined ? null : unitType.Abbreviation(), 2);
        }

        public DisplayUnit SelectDisplayUnit(UnitCategory unitCategory, IEnumerable<Quantity> quantities)
        {
            DisplayUnit result = DisplayUnit(unitCategory);
            if (unitCategory != UnitCategory.Power || quantities == null)
            {
                return result;
            }

            double max = 0;
            foreach (Quantity quantity in quantities)
            {
                if (!quantity.IsValid || quantity.Category != UnitCategory.Power)
                {
                    continue;
                }

                double value = System.Math.Abs(quantity.ConvertTo(result.UnitType).Value);
                if (value > max)
                {
                    max = value;
                }
            }

            if (UnitSystem == UnitStyle.SI)
            {
                return max >= KilowattThreshold_W ? new DisplayUnit(UnitType.Kilowatt, "kW", 2) : result;
            }

            return max >= KiloBtuPerHourThreshold_BtuPerHour ? new DisplayUnit(UnitType.KiloBtuPerHour, "kBtu/h", 1) : result;
        }

        public FormattedValue Format(ReportValue<Quantity> reportValue, DisplayUnit displayUnit)
        {
            if (reportValue == null)
            {
                throw new ArgumentNullException(nameof(reportValue));
            }

            if (!reportValue.TryGetValue(out Quantity quantity))
            {
                return Placeholder(reportValue);
            }

            if (displayUnit == null)
            {
                throw new ArgumentNullException(nameof(displayUnit));
            }

            double value = quantity.ConvertTo(displayUnit.UnitType).Value;
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new InvalidOperationException(string.Format("Cannot display {0} in {1}.", quantity, displayUnit));
            }

            return new FormattedValue(FormatNumber(value, displayUnit.Decimals), displayUnit.Symbol, reportValue.Availability, reportValue.Freshness, reportValue.Source, reportValue.SourceTimestamp, reportValue.Note);
        }

        public FormattedValue Format(ReportValue<Quantity> reportValue)
        {
            if (reportValue == null)
            {
                throw new ArgumentNullException(nameof(reportValue));
            }

            if (!reportValue.TryGetValue(out Quantity quantity))
            {
                return Placeholder(reportValue);
            }

            return Format(reportValue, SelectDisplayUnit(quantity.Category, new[] { quantity }));
        }

        public FormattedValue Format(ReportValue<string> reportValue)
        {
            if (reportValue == null)
            {
                throw new ArgumentNullException(nameof(reportValue));
            }

            if (!reportValue.TryGetValue(out string text))
            {
                return Placeholder(reportValue);
            }

            return new FormattedValue(text, null, reportValue.Availability, reportValue.Freshness, reportValue.Source, reportValue.SourceTimestamp, reportValue.Note);
        }

        public FormattedValue Format(ReportValue<DateTime> reportValue)
        {
            if (reportValue == null)
            {
                throw new ArgumentNullException(nameof(reportValue));
            }

            if (!reportValue.TryGetValue(out DateTime dateTime))
            {
                return Placeholder(reportValue);
            }

            return new FormattedValue(FormatDateTime(dateTime), null, reportValue.Availability, reportValue.Freshness, reportValue.Source, reportValue.SourceTimestamp, reportValue.Note);
        }

        /// <summary>
        /// Formats a number with a fixed number of decimals and the culture's separators. A value that rounds to zero
        /// prints as zero, never as "-0.0".
        /// </summary>
        private string FormatNumber(double value, int decimals)
        {
            double rounded = System.Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            if (rounded == 0)
            {
                rounded = 0;
            }

            return rounded.ToString("N" + decimals.ToString(CultureInfo.InvariantCulture), cultureInfo);
        }

        /// <summary>
        /// Formats a date and time as "25 Sep 2026 14:02". Month names are invariant (English) on purpose: culture
        /// month abbreviations come from ICU data that differs between machines, which would make documents and their
        /// snapshots machine-dependent.
        /// </summary>
        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("d MMM yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        private FormattedValue Placeholder(IReportValue reportValue)
        {
            string text = reportValue.Availability == Availability.NotApplicable ? NotApplicableText : NotAvailableText;
            return new FormattedValue(text, null, reportValue.Availability, note: reportValue.Note);
        }
    }
}
