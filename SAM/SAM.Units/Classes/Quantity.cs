// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Globalization;

namespace SAM.Units
{
    /// <summary>
    /// Immutable physical value: a number whose meaning is carried by its unit. Holds no formatting policy
    /// (precision, display unit, culture); <see cref="ToString"/> is for diagnostics only.
    /// </summary>
    public readonly struct Quantity
    {
        public Quantity(double value, UnitType unit)
        {
            Value = value;
            Unit = unit;
        }

        public double Value { get; }

        public UnitType Unit { get; }

        public UnitCategory Category => Query.UnitCategory(Unit);

        /// <summary>
        /// True when the value is finite and the unit is defined.
        /// </summary>
        public bool IsValid => !double.IsNaN(Value) && !double.IsInfinity(Value) && Unit != UnitType.Undefined;

        /// <summary>
        /// The same quantity expressed in <paramref name="unitType"/>. The value is NaN when the conversion is not
        /// supported (for example across categories).
        /// </summary>
        public Quantity ConvertTo(UnitType unitType)
        {
            return new Quantity(Convert.ByUnitType(Value, Unit, unitType), unitType);
        }

        public override string ToString()
        {
            string value = Value.ToString(CultureInfo.InvariantCulture);
            string abbreviation = Unit.Abbreviation();

            return string.IsNullOrEmpty(abbreviation) ? value : string.Format("{0} {1}", value, abbreviation);
        }
    }
}
