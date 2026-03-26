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
        /// Minimal flw rate [l/s]
        /// </summary>
        public double? MinFlowRate_Lps { get; private set; }

        public bool IncludeInFloorAreaCheck { get; private set; }

        public bool IsTerminalSpace { get; private set; }

        public bool ScaleSupplyWithVolume { get; private set; }

        public bool ScaleExtractAboveMinimum { get; private set; }

        public string DefaultFlowWeightBasis { get; private set; }

        public List<string> Synonyms { get; private set; }

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
            List<string> synonyms)
        {
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

