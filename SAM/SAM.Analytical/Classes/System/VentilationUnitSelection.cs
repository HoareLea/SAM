// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    /// <summary>
    /// The outcome of choosing a ventilation unit product for one dwelling's duty: the one selected, or
    /// an explicit refusal saying what was asked for and what the catalogue could do.
    /// <para>
    /// <b>There is no third answer</b>, exactly as <see cref="SystemCapabilitySelection"/> has none. A
    /// selection that could not be made never returns the nearest unit and never returns a default.
    /// Handing back an undersized unit would size a dwelling's plant below the airflow Approved Document
    /// F requires of it, and the model would report a selection while describing a building that cannot
    /// ventilate itself.
    /// </para>
    /// </summary>
    public class VentilationUnitSelection
    {
        private VentilationUnitSelection(VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor, double supplyDuty_Lps, double extractDuty_Lps, string reason)
        {
            Descriptor = ventilationUnitCapacityDescriptor;
            SupplyDuty_Lps = supplyDuty_Lps;
            ExtractDuty_Lps = extractDuty_Lps;
            Reason = reason;
        }

        /// <summary>The product chosen, or null where none was.</summary>
        public VentilationUnitCapacityDescriptor Descriptor { get; }

        /// <summary>The chosen product's identity, or null where none was chosen.</summary>
        public VentilationUnitReference VentilationUnitReference
        {
            get
            {
                return Descriptor?.VentilationUnitReference;
            }
        }

        /// <summary>Whether a product was chosen.</summary>
        public bool IsSelected
        {
            get
            {
                return Descriptor is not null;
            }
        }

        /// <summary>The supply duty [l/s] the selection was made against. Carried so a report does not have to re-derive it.</summary>
        public double SupplyDuty_Lps { get; }

        /// <summary>The extract duty [l/s] the selection was made against.</summary>
        public double ExtractDuty_Lps { get; }

        /// <summary>Why, in words, for a report or a log. Null on success.</summary>
        public string Reason { get; }

        /// <summary>
        /// The unused supply capacity [l/s] of the chosen product - design headroom, not a duty.
        /// <see cref="double.NaN"/> where nothing was chosen.
        /// <para>
        /// This is the number Approved Document O optimisation may spend by raising an individual room's
        /// design airflow. It is emphatically <b>not</b> something to spend automatically: a unit rated
        /// at 150 l/s serving a 115 l/s dwelling is a 115 l/s dwelling.
        /// </para>
        /// </summary>
        public double SupplyHeadroom_Lps
        {
            get
            {
                return Descriptor is null ? double.NaN : Descriptor.MaximumSupplyFlowRate_Lps - SupplyDuty_Lps;
            }
        }

        /// <summary>The unused extract capacity [l/s] of the chosen product. See <see cref="SupplyHeadroom_Lps"/>.</summary>
        public double ExtractHeadroom_Lps
        {
            get
            {
                return Descriptor is null ? double.NaN : Descriptor.MaximumExtractFlowRate_Lps - ExtractDuty_Lps;
            }
        }

        internal static VentilationUnitSelection Selected(VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor, double supplyDuty_Lps, double extractDuty_Lps)
        {
            return new VentilationUnitSelection(ventilationUnitCapacityDescriptor, supplyDuty_Lps, extractDuty_Lps, null);
        }

        internal static VentilationUnitSelection Refused(string reason, double supplyDuty_Lps = double.NaN, double extractDuty_Lps = double.NaN)
        {
            return new VentilationUnitSelection(null, supplyDuty_Lps, extractDuty_Lps, reason);
        }

        public override string ToString()
        {
            return IsSelected ? Descriptor.ToString() : string.Format("Not selected: {0}", Reason);
        }
    }
}
