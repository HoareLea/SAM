// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Analytical.Enums;
using SAM.Core;

namespace SAM.Analytical
{
    public class PartFSpaceData : SAMObject
    {
        public PartFSpaceData(
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
            double calculatedFlowRate_Lps)
            : base(name)
        {
            PartFType = partFType;
            PartFVentilationType = partFVentilationType;
            IsBedroom = isBedroom;
            MinFlowRate_Lps = minFlowRate_Lps;
            IncludeInFloorAreaCheck = includeInFloorAreaCheck;
            IsTerminalSpace = isTerminalSpace;
            ScaleSupplyWithVolume = scaleSupplyWithVolume;
            ScaleExtractAboveMinimum = scaleExtractAboveMinimum;
            DefaultFlowWeightBasis = defaultFlowWeightBasis;
            CalculatedFlowRate_Lps = CalculatedFlowRate_Lps;
        }

        public PartFSpaceData(PartFSpaceData partFSpaceData)
            : base(partFSpaceData)
        {
            if (partFSpaceData is not null)
            {
                CalculatedFlowRate_Lps = partFSpaceData.CalculatedFlowRate_Lps;
                DefaultFlowWeightBasis = partFSpaceData.DefaultFlowWeightBasis;
                IncludeInFloorAreaCheck = partFSpaceData.IncludeInFloorAreaCheck;
                IsBedroom = partFSpaceData.IsBedroom;
                IsTerminalSpace = partFSpaceData.IsTerminalSpace;
                MinFlowRate_Lps = partFSpaceData.MinFlowRate_Lps;
                PartFType = partFSpaceData.PartFType;
                PartFVentilationType = partFSpaceData.PartFVentilationType;
                ScaleExtractAboveMinimum = partFSpaceData.ScaleExtractAboveMinimum;
                ScaleSupplyWithVolume = partFSpaceData.ScaleSupplyWithVolume;
            }

        }

        public PartFSpaceData()
        {
        }

        public PartFSpaceData(PartFCategory partFCategory)
            : base(partFCategory?.Name)
        {
            CalculatedFlowRate_Lps = null;

            if (partFCategory is not null)
            {
                DefaultFlowWeightBasis = partFCategory.DefaultFlowWeightBasis;
                IncludeInFloorAreaCheck = partFCategory.IncludeInFloorAreaCheck;
                IsBedroom = partFCategory.IsBedroom;
                IsTerminalSpace = partFCategory.IsTerminalSpace;
                MinFlowRate_Lps = partFCategory.MinFlowRate_Lps;
                PartFType = partFCategory.PartFType;
                PartFVentilationType = partFCategory.PartFVentilationType;
                ScaleExtractAboveMinimum = partFCategory.ScaleExtractAboveMinimum;
                ScaleSupplyWithVolume = partFCategory.ScaleSupplyWithVolume;
            }
        }


        public PartFSpaceData(JObject jObject)
            : base(jObject)
        {
        }

        public double? CalculatedFlowRate_Lps { get; set; }

        public string DefaultFlowWeightBasis { get; private set; }

        public bool IncludeInFloorAreaCheck { get; private set; }

        public bool IsBedroom { get; private set; }

        public bool IsTerminalSpace { get; private set; }

        public double? MinFlowRate_Lps { get; private set; }

        public PartFType PartFType { get; private set; }

        public PartFVentilationType PartFVentilationType { get; private set; }

        public bool ScaleExtractAboveMinimum { get; private set; }

        public bool ScaleSupplyWithVolume { get; private set; }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if (!result)
            {
                return result;
            }

            if (jObject.ContainsKey("CalculatedFlowRate_Lps"))
            {
                CalculatedFlowRate_Lps = jObject.Value<double>("CalculatedFlowRate_Lps");
            }

            if (jObject.ContainsKey("DefaultFlowWeightBasis"))
            {
                DefaultFlowWeightBasis = jObject.Value<string>("DefaultFlowWeightBasis");
            }

            if (jObject.ContainsKey("IncludeInFloorAreaCheck"))
            {
                IncludeInFloorAreaCheck = jObject.Value<bool>("IncludeInFloorAreaCheck");
            }

            if (jObject.ContainsKey("IsBedroom"))
            {
                IsBedroom = jObject.Value<bool>("IsBedroom");
            }

            if (jObject.ContainsKey("IsTerminalSpace"))
            {
                IsTerminalSpace = jObject.Value<bool>("IsTerminalSpace");
            }

            if (jObject.ContainsKey("MinFlowRate_Lps"))
            {
                MinFlowRate_Lps = jObject.Value<double>("MinFlowRate_Lps");
            }

            if (jObject.ContainsKey("PartFType"))
            {
                PartFType = Core.Query.Enum<PartFType>(jObject.Value<string>("PartFType"));
            }

            if (jObject.ContainsKey("PartFVentilationType"))
            {
                PartFVentilationType = Core.Query.Enum<PartFVentilationType>(jObject.Value<string>("PartFVentilationType"));
            }

            if (jObject.ContainsKey("ScaleExtractAboveMinimum"))
            {
                ScaleExtractAboveMinimum = jObject.Value<bool>("ScaleExtractAboveMinimum");
            }

            if (jObject.ContainsKey("ScaleSupplyWithVolume"))
            {
                ScaleSupplyWithVolume = jObject.Value<bool>("ScaleSupplyWithVolume");
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result is null)
            {
                return result;
            }

            if (CalculatedFlowRate_Lps is not null && !double.IsNaN(CalculatedFlowRate_Lps.Value))
            {
                result.Add("CalculatedFlowRate_Lps", CalculatedFlowRate_Lps.Value);
            }

            if (DefaultFlowWeightBasis is not null)
            {
                result.Add("DefaultFlowWeightBasis", DefaultFlowWeightBasis);
            }

            result.Add("IncludeInFloorAreaCheck", IncludeInFloorAreaCheck);

            result.Add("IsBedroom", IsBedroom);

            result.Add("IsTerminalSpace", IsTerminalSpace);

            if (MinFlowRate_Lps is not null && !double.IsNaN(MinFlowRate_Lps.Value))
            {
                result.Add("MinFlowRate_Lps", MinFlowRate_Lps.Value);
            }

            result.Add("PartFType", PartFType.ToString());

            result.Add("PartFVentilationType", PartFVentilationType.ToString());

            result.Add("ScaleExtractAboveMinimum", ScaleExtractAboveMinimum);

            result.Add("ScaleSupplyWithVolume", ScaleSupplyWithVolume);

            return result;
        }
    }
}