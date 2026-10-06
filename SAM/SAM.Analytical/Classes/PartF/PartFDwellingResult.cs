// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// Outcome of sizing one dwelling to Approved Document F, Volume 1: Dwellings (2021 edition,
    /// England). In single dwelling mode one result is produced for the whole model; in zoned mode
    /// one result is produced for each selected dwelling zone.
    /// <para>
    /// Every rate exists at two conditions. The continuous design condition is the Approved Document F
    /// sizing case that all implemented minimums are checked against. The setback condition is a SAM
    /// reduced-operation mode obtained by scaling the continuous design rates by the rule set's setback
    /// factor, which is 30% of continuous design by default; it never reduces or replaces the continuous
    /// design calculation.
    /// </para>
    /// </summary>
    public class PartFDwellingResult
    {
        public PartFDwellingResult(string name)
        {
            Name = name;
        }

        /// <summary>Name of the dwelling zone, or null where the whole model is one dwelling.</summary>
        public string Name { get; private set; }

        /// <summary>Names of the spaces included in this dwelling, classified or not.</summary>
        public List<string> SpaceNames { get; set; } = [];

        /// <summary>Number of spaces classified as a bedroom, per Table 1.3 (page 10).</summary>
        public int BedroomCount { get; set; }

        /// <summary>
        /// Number of habitable rooms in the dwelling. A habitable room is one used for dwelling purposes
        /// but not solely a kitchen, utility room, bathroom, cellar or sanitary accommodation (Approved
        /// Document F, Volume 1, 2021 edition, Appendix A, page 36).
        /// <para>
        /// Drives Table 1.3 note 1 (page 10): a dwelling with exactly one habitable room takes 13 l/s.
        /// A bathroom, ensuite, utility room, sanitary accommodation, circulation space, store, plant room
        /// or void is not habitable and so does not increase this count.
        /// </para>
        /// </summary>
        public int HabitableRoomCount { get; set; }

        /// <summary>Names of the habitable rooms counted, so the count can be checked against the model.</summary>
        public List<string> HabitableRoomNames { get; set; } = [];

        /// <summary>
        /// True where Table 1.3 note 1 (page 10) was applied because the dwelling has exactly one
        /// habitable room, so <see cref="BedroomOrHabitableRate_Lps"/> is the note 1 rate rather than the
        /// Table 1.3 bedroom rate.
        /// </summary>
        public bool OneHabitableRoomRuleApplied { get; set; }

        /// <summary>
        /// Whole dwelling rate [l/s] set by the dwelling's rooms: either the Table 1.3 note 1 one
        /// habitable room rate, or the Table 1.3 rate for <see cref="BedroomCount"/>.
        /// </summary>
        public double BedroomOrHabitableRate_Lps { get; set; }

        /// <summary>Internal floor area [m2] counted by paragraph 1.24a (page 10).</summary>
        public double InternalFloorArea_M2 { get; set; }

        /// <summary>Table 1.3 (page 10) minimum rate [l/s] for <see cref="BedroomCount"/>.</summary>
        public double BedroomBasedRate_Lps { get; set; }

        /// <summary>Paragraph 1.24a (page 10) rate [l/s], i.e. 0.3 l/(s.m2) x internal floor area.</summary>
        public double AreaBasedRate_Lps { get; set; }

        /// <summary>Paragraph 1.24 (page 10) whole dwelling rate [l/s], the greater of the two above.</summary>
        public double WholeDwellingRate_Lps { get; set; }

        /// <summary>
        /// Sum of the Table 1.2 (page 10) minimum <b>high</b> rates [l/s] of the continuous extract
        /// terminals present.
        /// <para>
        /// Reported for information and deliberately NOT applied to
        /// <see cref="ContinuousDesignSystemRate_Lps"/>. Table 1.2's per-room figures are minimum high
        /// rates, assessed one room at a time on the high condition; its continuous requirement is that
        /// the <i>total</i> of continuous extract reaches the whole dwelling rate. Raising the continuous
        /// rate to this sum would systematically oversize normal continuous operation in a dwelling with
        /// several wet rooms.
        /// </para>
        /// </summary>
        public double WetRoomMinimumTotal_Lps { get; set; }

        /// <summary>
        /// Whole dwelling ventilation rate [l/s] at the continuous design condition, per paragraph 1.24
        /// (page 10) and paragraph 1.69 (page 16): the greater of the bedroom or one-habitable-room rate
        /// and the floor area based rate. Equal to <see cref="WholeDwellingRate_Lps"/>; held separately
        /// because it is the rate the terminals are actually distributed from.
        /// </summary>
        public double ContinuousDesignSystemRate_Lps { get; set; }

        /// <summary>
        /// Whole dwelling ventilation rate [l/s] at the setback operating condition, i.e.
        /// <see cref="ContinuousDesignSystemRate_Lps"/> multiplied by the rule set's setback factor.
        /// </summary>
        public double SetbackSystemRate_Lps { get; set; }

        /// <summary>The setback factor actually applied, so a result records its own operating basis.</summary>
        public double SetbackFlowRateFactor { get; set; }

        /// <summary>
        /// Whole dwelling ventilation rate [l/s] at the continuous design condition.
        /// <para>Retained as an alias of <see cref="ContinuousDesignSystemRate_Lps"/>: this property has
        /// always held the sizing rate and continues to.</para>
        /// </summary>
        public double FinalSystemRate_Lps
        {
            get { return ContinuousDesignSystemRate_Lps; }
            set { ContinuousDesignSystemRate_Lps = value; }
        }

        /// <summary>Sum of the continuous design supply terminal rates [l/s] assigned to this dwelling.</summary>
        public double TotalSupply_Lps { get; set; }

        /// <summary>Sum of the continuous design extract terminal rates [l/s] assigned to this dwelling.</summary>
        public double TotalExtract_Lps { get; set; }

        /// <summary>Sum of the setback supply terminal rates [l/s] assigned to this dwelling.</summary>
        public double TotalSetbackSupply_Lps { get; set; }

        /// <summary>Sum of the setback extract terminal rates [l/s] assigned to this dwelling.</summary>
        public double TotalSetbackExtract_Lps { get; set; }

        /// <summary>Spaces that could not be matched to a Part F room category.</summary>
        public List<string> UnclassifiedSpaceNames { get; set; } = [];

        /// <summary>Spaces left out of the paragraph 1.24a internal floor area, such as voids.</summary>
        public List<string> FloorAreaExcludedSpaceNames { get; set; } = [];

        /// <summary>Classified spaces that received no ventilation flow rate.</summary>
        public List<string> UnassignedSpaceNames { get; set; } = [];

        /// <summary>
        /// Spaces excluded from this dwelling because they are positively identified as belonging to no
        /// dwelling - communal circulation and explicitly non-dwelling spaces.
        /// </summary>
        public List<string> NonDwellingSpaceNames { get; set; } = [];

        /// <summary>Conditions that need the engineer's attention.</summary>
        public List<string> Warnings { get; set; } = [];

        /// <summary>Informational notes about expected but noteworthy conditions.</summary>
        public List<string> Remarks { get; set; } = [];

        /// <summary>
        /// Sum of the high rate supply terminal rates [l/s] in the balanced mechanical ventilation with
        /// heat recovery flow.
        /// </summary>
        public double TotalHighSupply_Lps { get; set; }

        /// <summary>
        /// Sum of the high rate extract terminal rates [l/s] in the balanced mechanical ventilation with
        /// heat recovery flow. Extract from an intermittent device such as a cooker hood is excluded,
        /// because it does not run as part of the balanced system.
        /// </summary>
        public double TotalHighExtract_Lps { get; set; }

        /// <summary>
        /// Sum of the design rates [l/s] of extract terminals that are NOT part of the balanced flow -
        /// cooker hoods and separate intermittent extract fans - assessed against Table 1.1 rather than
        /// Table 1.2 and reported separately so they are never mistaken for continuous extract.
        /// </summary>
        public double TotalIntermittentExtract_Lps { get; set; }

        /// <summary>
        /// The Part F conformance assessment of this dwelling: the terminal, transfer, purge and
        /// commissioning schedules, every clause-level check, and the overall status they add up to.
        /// </summary>
        public PartFComplianceResult ComplianceResult { get; set; }
    }
}
