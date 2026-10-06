// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Units;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// A unit a value is displayed in: the conversion target, its display symbol and the number of decimals.
    /// Chosen by <see cref="IQuantityFormatter"/>, once per group of comparable values.
    /// </summary>
    public sealed class DisplayUnit
    {
        public DisplayUnit(UnitType unitType, string symbol, int decimals)
        {
            UnitType = unitType;
            Symbol = symbol;
            Decimals = decimals < 0 ? 0 : decimals;
        }

        public UnitType UnitType { get; }

        /// <summary>
        /// Display symbol, for example "m²" or "Btu/h·ft²". Null for a dimensionless value shown without a unit.
        /// </summary>
        public string Symbol { get; }

        public int Decimals { get; }

        public override string ToString()
        {
            return string.Format("{0} ({1} dp)", Symbol ?? UnitType.ToString(), Decimals);
        }
    }
}
