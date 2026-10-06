// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class PartFCategory
    {
        public string Name { get; private set; }

        public PartFType PartFType { get; private set; }

        public PartFVentilationType PartFVentilationType { get; private set; }

        public bool IsBedroom { get; private set; }

        /// <summary>
        /// Minimum extract ventilation rate [l/s] for a CONTINUOUS extract system, i.e. the high rate
        /// column of Approved Document F, Volume 1: Dwellings (2021 edition, England) Table 1.2 (page 10):
        /// kitchen 13, utility room 8, bathroom 8, sanitary accommodation 6.
        /// <para>
        /// Table 1.2 note 1: where the continuous rate provided in a room is equal to or higher than this
        /// minimum high rate, no extra ventilation is needed. It is therefore a floor on the high rate,
        /// not a target for the continuous rate.
        /// </para>
        /// </summary>
        public double? MinFlowRate_Lps { get; private set; }

        /// <summary>
        /// Minimum extract ventilation rate [l/s] for an INTERMITTENT extract system, from Table 1.1
        /// (page 8): utility room 30, bathroom 15, sanitary accommodation 6.
        /// <para>
        /// Null for a kitchen, because Table 1.1 gives a kitchen two different rates depending on
        /// something no room category can know: 30 l/s where a cooker hood extracts to the outside, and
        /// 60 l/s where there is no cooker hood or the hood does not extract to the outside. Those are
        /// selected from the terminal's extract method instead, using
        /// <see cref="PartFData.IntermittentKitchenRateWithCookerHood_Lps"/> and
        /// <see cref="PartFData.IntermittentKitchenRateWithoutCookerHood_Lps"/>.
        /// </para>
        /// </summary>
        public double? IntermittentExtractRate_Lps { get; private set; }

        public bool IncludeInFloorAreaCheck { get; private set; }

        public bool IsTerminalSpace { get; private set; }

        public bool ScaleSupplyWithVolume { get; private set; }

        public bool ScaleExtractAboveMinimum { get; private set; }

        public string DefaultFlowWeightBasis { get; private set; }

        public List<string> Synonyms { get; private set; }

        /// <summary>
        /// True where the room contains the cooking function, so Approved Document F, Volume 1
        /// (2021 edition) paragraph 1.17 and Table 1.2 require kitchen extract from it. A studio and
        /// an open plan living kitchen are both cooking spaces.
        /// </summary>
        public bool IsCookingSpace { get; private set; }

        /// <summary>
        /// The shared semantic classification this Approved Document F category applies to. This is
        /// what links the Part F rule set to the vocabulary that Approved Document O, CIBSE TM59 and
        /// the SAM_UI internal condition mapping also use, so a space is recognised once and every
        /// standard agrees what it is.
        /// <para>
        /// <see cref="Analytical.SpaceUse.Undefined"/> where a rule set predates the shared vocabulary;
        /// such a category is matched by its <see cref="Synonyms"/> alone.
        /// </para>
        /// </summary>
        public SpaceUse SpaceUse { get; private set; }

        public PartFCategory(
            string name,
            PartFType partFType,
            PartFVentilationType partFVentilationType,
            bool isBedroom,
            double? minFlowRate_Lps,
            bool includeInFloorAreaCheck,
            bool isTerminalSpace,
            bool scaleSupplyWithVolume,
            bool scaleExtractAboveMinimum,
            string defaultFlowWeightBasis,
            List<string> synonyms,
            bool isCookingSpace = false,
            SpaceUse spaceUse = SpaceUse.Undefined,
            double? intermittentExtractRate_Lps = null)
        {
            IntermittentExtractRate_Lps = intermittentExtractRate_Lps;
            IsCookingSpace = isCookingSpace;
            SpaceUse = spaceUse;
            Name = name;
            PartFType = partFType;
            PartFVentilationType = partFVentilationType;
            IsBedroom = isBedroom;
            MinFlowRate_Lps = minFlowRate_Lps;
            IncludeInFloorAreaCheck = includeInFloorAreaCheck;
            IsTerminalSpace = isTerminalSpace;
            ScaleSupplyWithVolume = scaleSupplyWithVolume;
            ScaleExtractAboveMinimum = scaleExtractAboveMinimum;
            DefaultFlowWeightBasis = defaultFlowWeightBasis;
            Synonyms = synonyms;
        }
    }
}

