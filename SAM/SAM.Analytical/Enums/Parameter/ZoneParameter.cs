// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Zone)), Description("Analytical Zone Parameter")]
    public enum ZoneParameter
    {
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,

        [ParameterProperties("Zone Category", "Zone Category"), ParameterValue(Core.ParameterType.String)] ZoneCategory,

        /// <summary>
        /// True where the zone represents one dwelling - a self-contained unit designed to accommodate
        /// a single household (Approved Document F, Volume 1, 2021 edition, Appendix A).
        /// <para>
        /// A zone category on its own cannot answer this: a shared corridor, a landlord area or a
        /// commercial unit can legitimately sit in the same category as the flats it serves, and sizing
        /// it as a dwelling produces meaningless ventilation rates. Set this to false on those zones,
        /// and true on each flat or house.
        /// </para>
        /// <para>
        /// A zone that has never had the parameter set is not the same as one explicitly set to false.
        /// Consumers should treat only an explicit true as a dwelling, report zones that carry no value
        /// at all, and never size a zone explicitly set to false.
        /// </para>
        /// </summary>
        [ParameterProperties("Is Dwelling", "Is Dwelling"), ParameterValue(Core.ParameterType.Boolean)] IsDwelling,

        /// <summary>
        /// Commissioning evidence for the dwelling this zone represents, following Approved Document F,
        /// Volume 1 (2021 edition) Section 4 and Appendix C.
        /// <para>
        /// Held on the zone because commissioning is per dwelling, not per space or per terminal: one
        /// commissioning sheet covers one installation address. Per terminal measured air flow rates live
        /// on the terminals themselves.
        /// </para>
        /// </summary>
        [ParameterProperties("PartF Commissioning Data", "PartF Commissioning Data"), SAMObjectParameterValue(typeof(PartFCommissioningData))] PartFCommissioningData,
    }
}
