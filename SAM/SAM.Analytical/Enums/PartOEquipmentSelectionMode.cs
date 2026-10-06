// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// How the ventilation unit assigned to each dwelling at Iteration 2 was <b>arrived at</b> - the
    /// selection <i>authority</i>, and nothing else.
    /// <para>
    /// <b>This is not an assignment and it is not a capability.</b> What a dwelling is fitted with is
    /// <c>AirHandlingUnitParameter.VentilationUnitReference</c> on its own air handling unit; what that
    /// product can move is a catalogue fact (<see cref="VentilationUnitCapacityDescriptor"/>). This type
    /// only records who decided - the smallest-capable rule, or the engineer. Reading a mode as though it
    /// were an assignment is how a dwelling ends up "selected as Automatic".
    /// </para>
    /// <para>
    /// <b>A mode describes how the initial assignment was produced, not a standing instruction to keep
    /// re-selecting.</b> Automatic selection runs once, as part of preparing Iteration 2. Iteration 2B
    /// then consumes the identities it produced and treats their capacities as ceilings; it does not
    /// re-run the rule when design airflow changes. See <c>Modify.PreparePartOIteration</c> and
    /// <c>OptimisePartOTM59</c>.
    /// </para>
    /// </summary>
    [Description("Part O Equipment Selection Mode.")]
    public enum PartOEquipmentSelectionMode
    {
        /// <summary>
        /// Every selectable product in the catalogue is a candidate, and each dwelling is assigned the
        /// smallest one that can meet its design duty. The behaviour Iteration 2 has always had, and the
        /// default for a project that has never said otherwise.
        /// </summary>
        [Description("Automatic - all catalogue products")] AutomaticAllProducts,

        /// <summary>
        /// Only the products in <see cref="PartOEquipmentSelection.AllowedVentilationUnitReferences"/> are
        /// candidates; the smallest-capable rule then runs over exactly those.
        /// <para>
        /// <b>An empty pool refuses.</b> The engineer has said "choose from what I have permitted" and
        /// permitted nothing, so there is no candidate to choose - which is reported, never quietly
        /// widened back to the whole catalogue. That silent widening is the failure this mode exists to
        /// make impossible, so it has no fallback.
        /// </para>
        /// </summary>
        [Description("Automatic - selected pool")] AutomaticSelectedPool,

        /// <summary>
        /// The engineer states each dwelling's product. No selection rule runs at all: preparation is
        /// given no catalogue to select from, so whatever identity each air handling unit already carries
        /// is preserved exactly, and changes are the ones the engineer makes.
        /// <para>
        /// A manual assignment may deliberately be a <i>larger</i> capable product than the rule would
        /// have picked. That is a valid engineering decision and is never corrected back.
        /// </para>
        /// <para>
        /// Here <see cref="PartOEquipmentSelection.AllowedVentilationUnitReferences"/> is a
        /// <i>preselection</i> - the normal candidate list a dwelling's product is chosen from - and an
        /// empty one means no preselection has been made rather than "nothing is permitted". See
        /// <see cref="PartOEquipmentSelection.AllowedDescriptors"/>.
        /// </para>
        /// </summary>
        [Description("Manual per dwelling")] ManualPerDwelling,
    }
}
