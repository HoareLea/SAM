// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// How an extract terminal actually removes air, which decides both the rate table that applies
    /// and whether the terminal forms part of the balanced mechanical ventilation with heat recovery
    /// design flow.
    /// <para>
    /// Approved Document F, Volume 1: Dwellings (2021 edition, England) paragraph 1.17 requires extract
    /// ventilation <b>to the outside</b>. Paragraph 1.18 allows it to be intermittent or continuous, and
    /// paragraph 1.19 sends intermittent systems to Table 1.1 and continuous systems to Table 1.2. The
    /// method therefore cannot be inferred from the room alone.
    /// </para>
    /// </summary>
    public enum PartFExtractMethod
    {
        /// <summary>
        /// No extract provision is represented for this location. Approved Document F paragraph 1.17
        /// still requires one, so this is reported rather than treated as compliant.
        /// </summary>
        [Description("Not Represented")] NotRepresented,

        /// <summary>
        /// A continuous mechanical extract terminal on the mechanical ventilation with heat recovery
        /// system. Assessed against Table 1.2 (page 10) and paragraph 1.70 (page 17), and counted in the
        /// balanced continuous and high design flows.
        /// </summary>
        [Description("MVHR Continuous Terminal")] MVHRContinuousTerminal,

        /// <summary>
        /// A cooker hood that extracts to the outside. Assessed against Table 1.1 (page 8) at 30 l/s
        /// intermittent, with the hood height of paragraph 1.21 (page 8). Intermittent operation, so it
        /// is NOT counted in the balanced continuous mechanical ventilation with heat recovery flow.
        /// </summary>
        [Description("Cooker Hood Extracting Outside")] CookerHoodExtractingOutside,

        /// <summary>
        /// A separate intermittent extract fan discharging outside. Assessed against Table 1.1 (page 8);
        /// for a kitchen with no cooker hood extracting outside that is 60 l/s. Intermittent operation,
        /// so it is NOT counted in the balanced continuous mechanical ventilation with heat recovery flow.
        /// </summary>
        [Description("Separate Intermittent Extract")] SeparateIntermittentExtract,

        /// <summary>
        /// Another explicitly represented arrangement that extracts to the outside. The rate and the
        /// applicable table cannot be established automatically, so this always requires engineering
        /// review, and it is not counted in the balanced flow.
        /// </summary>
        [Description("Other Explicit External Extract")] OtherExplicitExternalExtract,

        /// <summary>
        /// A recirculating cooker hood. Approved Document F Diagram 1.2 note 1 (page 9) states that a
        /// recirculating cooker hood on its own does not provide a means of ventilation that complies
        /// with Part F, so this never satisfies the extract requirement and is never counted in any
        /// design flow.
        /// </summary>
        [Description("Recirculating Cooker Hood")] RecirculatingCookerHood,

        /// <summary>
        /// Nothing has been recorded about how extract is provided at this location.
        /// <para>
        /// Deliberately distinct from <see cref="NotRepresented"/>. That value is a positive statement
        /// that there is no provision, which is a calculable failure. This one is the absence of any
        /// statement either way, which is not: the requirement stands, the design may well satisfy it,
        /// and nobody has said. It is the value
        /// <see cref="PartFVentilationTerminalRequirement.ProvidedExtractMethod"/> holds until the model
        /// or a person supplies the actual arrangement, and it never reaches a pass, because absence of
        /// evidence is not compliance.
        /// </para>
        /// </summary>
        [Description("Not Specified")] NotSpecified,
    }
}
