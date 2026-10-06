// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The Approved Document F result for one space: what the room is, which terminals it needs, the
    /// rates for each of them, and its purge ventilation assessment.
    /// <para>
    /// <b>One space, several terminals.</b> A room can require more than one ventilation terminal.
    /// Approved Document F, Volume 1: Dwellings (2021 edition, England) Appendix A (page 36) makes a
    /// studio and an open plan living kitchen habitable rooms, because neither is <i>solely</i> a
    /// kitchen, so paragraph 1.67 (page 16) requires mechanical supply to them; and both contain the
    /// cooking function, so paragraph 1.17a (page 8) and Table 1.2 (page 10) require kitchen extract
    /// from them too. <see cref="Terminals"/> holds them all.
    /// </para>
    /// <para>
    /// <b>Backward compatibility.</b> <see cref="CalculatedFlowRate_Lps"/>,
    /// <see cref="ContinuousDesignFlowRate_Lps"/>, <see cref="SetbackFlowRate_Lps"/> and
    /// <see cref="PartFVentilationType"/> keep their original meaning: they describe the space's PRIMARY
    /// terminal, which is the supply terminal of a habitable room and the extract terminal of a wet room.
    /// A consumer written before terminal-level sizing therefore reads exactly the rate it always read.
    /// The secondary terminal of a multi-terminal space - the local kitchen extract of a studio or living
    /// kitchen - is not visible through those scalars, but it is never discarded: it is serialised in
    /// <see cref="Terminals"/> and exposed through <see cref="LocalKitchenExtractFlowRate_Lps"/>. New
    /// work should read <see cref="Terminals"/>.
    /// </para>
    /// </summary>
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
            double calculatedFlowRate_Lps,
            bool isCookingSpace = false,
            SpaceUse spaceUse = SpaceUse.Undefined,
            double? setbackFlowRate_Lps = null)
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
            ContinuousDesignFlowRate_Lps = calculatedFlowRate_Lps;
            IsCookingSpace = isCookingSpace;
            SpaceUse = spaceUse;
            SetbackFlowRate_Lps = setbackFlowRate_Lps;
        }

        public PartFSpaceData(PartFSpaceData partFSpaceData)
            : base(partFSpaceData)
        {
            if (partFSpaceData is not null)
            {
                ContinuousDesignFlowRate_Lps = partFSpaceData.ContinuousDesignFlowRate_Lps;
                SetbackFlowRate_Lps = partFSpaceData.SetbackFlowRate_Lps;
                DefaultFlowWeightBasis = partFSpaceData.DefaultFlowWeightBasis;
                IncludeInFloorAreaCheck = partFSpaceData.IncludeInFloorAreaCheck;
                IsBedroom = partFSpaceData.IsBedroom;
                IsTerminalSpace = partFSpaceData.IsTerminalSpace;
                MinFlowRate_Lps = partFSpaceData.MinFlowRate_Lps;
                PartFType = partFSpaceData.PartFType;
                PartFVentilationType = partFSpaceData.PartFVentilationType;
                ScaleExtractAboveMinimum = partFSpaceData.ScaleExtractAboveMinimum;
                ScaleSupplyWithVolume = partFSpaceData.ScaleSupplyWithVolume;
                IsCookingSpace = partFSpaceData.IsCookingSpace;
                SpaceUse = partFSpaceData.SpaceUse;

                foreach (PartFVentilationTerminalRequirement terminal in partFSpaceData.Terminals ?? [])
                {
                    Terminals.Add(new PartFVentilationTerminalRequirement(terminal));
                }

                if (partFSpaceData.Purge is not null)
                {
                    Purge = new PartFPurgeVentilationData(partFSpaceData.Purge);
                }
            }
        }

        public PartFSpaceData()
        {
        }

        public PartFSpaceData(PartFCategory partFCategory)
            : base(partFCategory?.Name)
        {
            ContinuousDesignFlowRate_Lps = null;
            SetbackFlowRate_Lps = null;

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
                IsCookingSpace = partFCategory.IsCookingSpace;
                SpaceUse = partFCategory.SpaceUse;
            }
        }

        public PartFSpaceData(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// Ventilation flow rate [l/s] at the continuous design condition: the Approved Document F
        /// sizing case that every implemented minimum is checked against. Equipment is selected on this
        /// rate.
        /// </summary>
        public double? ContinuousDesignFlowRate_Lps { get; set; }

        /// <summary>
        /// Ventilation flow rate [l/s] at the setback operating condition, i.e.
        /// <see cref="ContinuousDesignFlowRate_Lps"/> scaled by the rule set's setback factor, which is
        /// 30% of the continuous design rate by default.
        /// <para>
        /// A SAM reduced-operation convention, not a regulatory rate. It does not reduce or replace the
        /// continuous design calculation and is not checked against the Table 1.2 minimums, which apply
        /// at the continuous design condition.
        /// </para>
        /// <para>
        /// Deliberately not called a background rate: in Approved Document F a background ventilator is a
        /// trickle ventilator, and "whole dwelling (general) ventilation" is the continuous requirement,
        /// so "background" would be ambiguous against both.
        /// </para>
        /// </summary>
        public double? SetbackFlowRate_Lps { get; set; }

        /// <summary>
        /// The continuous design ventilation flow rate [l/s].
        /// <para>
        /// Retained as an alias of <see cref="ContinuousDesignFlowRate_Lps"/> so that existing scripts,
        /// Grasshopper definitions and serialised models keep their original meaning: this property has
        /// always held the sizing rate and continues to. It is read externally by SAM_Tas_Grasshopper and
        /// SAM_Systems, so its meaning is deliberately unchanged. New work should read
        /// <see cref="ContinuousDesignFlowRate_Lps"/> or <see cref="SetbackFlowRate_Lps"/> explicitly.
        /// </para>
        /// </summary>
        public double? CalculatedFlowRate_Lps
        {
            get
            {
                return ContinuousDesignFlowRate_Lps;
            }

            set
            {
                ContinuousDesignFlowRate_Lps = value;
            }
        }

        public string DefaultFlowWeightBasis { get; private set; }

        public bool IncludeInFloorAreaCheck { get; private set; }

        public bool IsBedroom { get; private set; }

        /// <summary>
        /// True where the room contains the cooking function, so Approved Document F, Volume 1
        /// (2021 edition) paragraph 1.17a and Table 1.2 require kitchen extract from it.
        /// </summary>
        public bool IsCookingSpace { get; private set; }

        public bool IsTerminalSpace { get; private set; }

        public double? MinFlowRate_Lps { get; private set; }

        public PartFType PartFType { get; private set; }

        public PartFVentilationType PartFVentilationType { get; private set; }

        public bool ScaleExtractAboveMinimum { get; private set; }

        public bool ScaleSupplyWithVolume { get; private set; }

        /// <summary>
        /// The shared semantic classification the Approved Document F category was resolved from, kept
        /// so a result can be traced back to how the room was recognised.
        /// </summary>
        public SpaceUse SpaceUse { get; private set; }

        // ------------------------------------------------------------------
        // Terminal collection
        // ------------------------------------------------------------------

        /// <summary>
        /// Every ventilation terminal this space requires. A habitable room with the cooking function
        /// carries two: a supply terminal under paragraph 1.67 and a local kitchen extract terminal under
        /// paragraph 1.17a. This is the authoritative representation; the scalar rate properties above
        /// describe only the primary terminal and exist for consumers written before this collection did.
        /// </summary>
        public List<PartFVentilationTerminalRequirement> Terminals { get; set; } = [];

        /// <summary>
        /// The purge ventilation assessment of this room, per paragraphs 1.26 to 1.31 (page 11). Null
        /// where the room is not habitable, since paragraph 1.26 requires purge ventilation in habitable
        /// rooms only.
        /// </summary>
        public PartFPurgeVentilationData Purge { get; set; }

        /// <summary>
        /// The terminal the scalar rate properties describe: the supply terminal of a habitable room, the
        /// extract terminal of a wet room. Null where the space has no terminal at all.
        /// </summary>
        public PartFVentilationTerminalRequirement PrimaryTerminal()
        {
            if (Terminals is null || Terminals.Count == 0)
            {
                return null;
            }

            PartFTerminalRole role_Primary = PartFVentilationType == Enums.PartFVentilationType.supply
                ? PartFTerminalRole.Supply
                : PartFTerminalRole.GeneralExtract;

            PartFVentilationTerminalRequirement result = Terminals.Find(x => x is not null && x.TerminalRole == role_Primary);

            //A room that is solely a kitchen carries its extract as LOCAL kitchen extract rather than
            //general extract, so it has no GeneralExtract terminal to be its primary one. Its extract
            //terminal is still what the legacy scalar has always reported.
            result ??= Terminals.Find(x => x is not null && x.TerminalRole == PartFTerminalRole.LocalKitchenExtract && x.IsInBalancedFlow);

            return result ?? Terminals[0];
        }

        /// <summary>Continuous design supply into this space [l/s], or null where it has no supply terminal.</summary>
        public double? ContinuousSupplyFlowRate_Lps
        {
            get { return Sum(PartFTerminalRole.Supply, x => x.ContinuousDesignFlowRate_Lps); }
        }

        /// <summary>
        /// Continuous design extract out of this space [l/s], general and local kitchen extract together,
        /// or null where it has no extract terminal that runs continuously.
        /// </summary>
        public double? ContinuousExtractFlowRate_Lps
        {
            get
            {
                double? general = Sum(PartFTerminalRole.GeneralExtract, x => x.ContinuousDesignFlowRate_Lps);
                double? local = Sum(PartFTerminalRole.LocalKitchenExtract, x => x.ContinuousDesignFlowRate_Lps);

                return general is null && local is null ? null : (general ?? 0) + (local ?? 0);
            }
        }

        /// <summary>
        /// Continuous design extract local to the cooking function [l/s], held separately from general
        /// wet room extract because extract from a bathroom or ensuite is not local kitchen extract.
        /// </summary>
        public double? LocalKitchenExtractFlowRate_Lps
        {
            get { return Sum(PartFTerminalRole.LocalKitchenExtract, x => x.ContinuousDesignFlowRate_Lps); }
        }

        /// <summary>High rate supply into this space [l/s].</summary>
        public double? HighSupplyFlowRate_Lps
        {
            get { return Sum(PartFTerminalRole.Supply, x => x.HighFlowRate_Lps); }
        }

        /// <summary>High rate extract out of this space [l/s], general and local kitchen extract together.</summary>
        public double? HighExtractFlowRate_Lps
        {
            get
            {
                double? general = Sum(PartFTerminalRole.GeneralExtract, x => x.HighFlowRate_Lps);
                double? local = Sum(PartFTerminalRole.LocalKitchenExtract, x => x.HighFlowRate_Lps);

                return general is null && local is null ? null : (general ?? 0) + (local ?? 0);
            }
        }

        /// <summary>Setback supply into this space [l/s].</summary>
        public double? SetbackSupplyFlowRate_Lps
        {
            get { return Sum(PartFTerminalRole.Supply, x => x.SetbackFlowRate_Lps); }
        }

        /// <summary>Setback extract out of this space [l/s], general and local kitchen extract together.</summary>
        public double? SetbackExtractFlowRate_Lps
        {
            get
            {
                double? general = Sum(PartFTerminalRole.GeneralExtract, x => x.SetbackFlowRate_Lps);
                double? local = Sum(PartFTerminalRole.LocalKitchenExtract, x => x.SetbackFlowRate_Lps);

                return general is null && local is null ? null : (general ?? 0) + (local ?? 0);
            }
        }

        /// <summary>
        /// Net continuous design airflow [l/s] into this space: supply less all extract. Positive where
        /// the space must pass air on to somewhere else, negative where it must draw air in.
        /// <para>
        /// Only terminals in the balanced mechanical ventilation with heat recovery flow are counted. An
        /// intermittent cooker hood does not run at the continuous design condition, so counting it here
        /// would make the dwelling's transfer air balance describe a condition that never occurs.
        /// </para>
        /// </summary>
        public double NetContinuousFlowRate_Lps
        {
            get
            {
                double result = 0;

                foreach (PartFVentilationTerminalRequirement terminal in Terminals ?? [])
                {
                    if (terminal is null || !terminal.IsInBalancedFlow || terminal.ContinuousDesignFlowRate_Lps is null)
                    {
                        continue;
                    }

                    result += terminal.IsExtract ? -terminal.ContinuousDesignFlowRate_Lps.Value : terminal.ContinuousDesignFlowRate_Lps.Value;
                }

                return result;
            }
        }

        private double? Sum(PartFTerminalRole partFTerminalRole, System.Func<PartFVentilationTerminalRequirement, double?> func)
        {
            List<PartFVentilationTerminalRequirement> terminals = (Terminals ?? []).FindAll(x => x is not null && x.TerminalRole == partFTerminalRole && func(x) is not null);

            //Null rather than zero where there is no such terminal: "this space has no supply" and "this
            //space is supplied 0 l/s" are different answers and a schedule must be able to tell them
            //apart.
            return terminals.Count == 0 ? null : terminals.Sum(x => func(x).Value);
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            bool result = base.FromJsonObject(jsonObject);
            if (!result)
            {
                return result;
            }

            //The continuous design rate is read from whichever key is present, newest name first. A model
            //serialised before the two conditions were separated carries only CalculatedFlowRate_Lps, and
            //that value has always been the sizing rate. DesignFlowRate_Lps is accepted too, for models
            //written by the interim build that used that name.
            if (jsonObject.ContainsKey("ContinuousDesignFlowRate_Lps"))
            {
                ContinuousDesignFlowRate_Lps = jsonObject["ContinuousDesignFlowRate_Lps"]?.GetValue<double>() ?? double.NaN;
            }
            else if (jsonObject.ContainsKey("DesignFlowRate_Lps"))
            {
                ContinuousDesignFlowRate_Lps = jsonObject["DesignFlowRate_Lps"]?.GetValue<double>() ?? double.NaN;
            }
            else if (jsonObject.ContainsKey("CalculatedFlowRate_Lps"))
            {
                ContinuousDesignFlowRate_Lps = jsonObject["CalculatedFlowRate_Lps"]?.GetValue<double>() ?? double.NaN;
            }

            if (jsonObject.ContainsKey("SetbackFlowRate_Lps"))
            {
                SetbackFlowRate_Lps = jsonObject["SetbackFlowRate_Lps"]?.GetValue<double>() ?? double.NaN;
            }
            else if (jsonObject.ContainsKey("BackgroundFlowRate_Lps"))
            {
                SetbackFlowRate_Lps = jsonObject["BackgroundFlowRate_Lps"]?.GetValue<double>() ?? double.NaN;
            }

            if (jsonObject.ContainsKey("DefaultFlowWeightBasis"))
            {
                DefaultFlowWeightBasis = jsonObject["DefaultFlowWeightBasis"]?.GetValue<string>();
            }

            if (jsonObject.ContainsKey("IncludeInFloorAreaCheck"))
            {
                IncludeInFloorAreaCheck = jsonObject["IncludeInFloorAreaCheck"]?.GetValue<bool>() ?? false;
            }

            if (jsonObject.ContainsKey("IsBedroom"))
            {
                IsBedroom = jsonObject["IsBedroom"]?.GetValue<bool>() ?? false;
            }

            if (jsonObject.ContainsKey("IsCookingSpace"))
            {
                IsCookingSpace = jsonObject["IsCookingSpace"]?.GetValue<bool>() ?? false;
            }

            if (jsonObject.ContainsKey("IsTerminalSpace"))
            {
                IsTerminalSpace = jsonObject["IsTerminalSpace"]?.GetValue<bool>() ?? false;
            }

            if (jsonObject.ContainsKey("MinFlowRate_Lps"))
            {
                MinFlowRate_Lps = jsonObject["MinFlowRate_Lps"]?.GetValue<double>() ?? double.NaN;
            }

            if (jsonObject.ContainsKey("PartFType"))
            {
                PartFType = Core.Query.Enum<PartFType>(jsonObject["PartFType"]?.GetValue<string>());
            }

            if (jsonObject.ContainsKey("PartFVentilationType"))
            {
                PartFVentilationType = Core.Query.Enum<PartFVentilationType>(jsonObject["PartFVentilationType"]?.GetValue<string>());
            }

            if (jsonObject.ContainsKey("ScaleExtractAboveMinimum"))
            {
                ScaleExtractAboveMinimum = jsonObject["ScaleExtractAboveMinimum"]?.GetValue<bool>() ?? false;
            }

            if (jsonObject.ContainsKey("ScaleSupplyWithVolume"))
            {
                ScaleSupplyWithVolume = jsonObject["ScaleSupplyWithVolume"]?.GetValue<bool>() ?? false;
            }

            if (jsonObject.ContainsKey("SpaceUse"))
            {
                SpaceUse = Core.Query.Enum<SpaceUse>(jsonObject["SpaceUse"]?.GetValue<string>());
            }

            //A model written before terminal-level sizing carries no Terminals array. It is left empty
            //rather than reconstructed from the scalar rate: the scalar cannot say whether a studio's
            //33 l/s was supply with a separate local kitchen extract or supply alone, and inventing a
            //terminal that the original calculation never established would be worse than having none.
            Terminals = [];
            if (jsonObject["Terminals"] is JsonArray jsonArray)
            {
                foreach (JsonNode jsonNode in jsonArray)
                {
                    if (jsonNode is JsonObject jsonObject_Terminal)
                    {
                        Terminals.Add(new PartFVentilationTerminalRequirement(jsonObject_Terminal));
                    }
                }
            }

            if (jsonObject["Purge"] is JsonObject jsonObject_Purge)
            {
                Purge = new PartFPurgeVentilationData(jsonObject_Purge);
            }

            return result;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            if (ContinuousDesignFlowRate_Lps is not null && !double.IsNaN(ContinuousDesignFlowRate_Lps.Value))
            {
                result["ContinuousDesignFlowRate_Lps"] = ContinuousDesignFlowRate_Lps.Value;

                //Also written under the original key so a model produced here can still be read by an
                //earlier SAM build, which only knows CalculatedFlowRate_Lps.
                result["CalculatedFlowRate_Lps"] = ContinuousDesignFlowRate_Lps.Value;
            }

            if (SetbackFlowRate_Lps is not null && !double.IsNaN(SetbackFlowRate_Lps.Value))
            {
                result["SetbackFlowRate_Lps"] = SetbackFlowRate_Lps.Value;
            }

            if (DefaultFlowWeightBasis is not null)
            {
                result["DefaultFlowWeightBasis"] = DefaultFlowWeightBasis;
            }

            result["IncludeInFloorAreaCheck"] = IncludeInFloorAreaCheck;

            result["IsBedroom"] = IsBedroom;

            result["IsCookingSpace"] = IsCookingSpace;

            result["IsTerminalSpace"] = IsTerminalSpace;

            if (MinFlowRate_Lps is not null && !double.IsNaN(MinFlowRate_Lps.Value))
            {
                result["MinFlowRate_Lps"] = MinFlowRate_Lps.Value;
            }

            result["PartFType"] = PartFType.ToString();

            result["PartFVentilationType"] = PartFVentilationType.ToString();

            result["ScaleExtractAboveMinimum"] = ScaleExtractAboveMinimum;

            result["ScaleSupplyWithVolume"] = ScaleSupplyWithVolume;

            result["SpaceUse"] = SpaceUse.ToString();

            //Written whenever the space has any terminal at all, including the secondary local kitchen
            //extract terminal that the legacy scalar rate above cannot express. A secondary terminal is
            //never dropped on the way to file.
            if (Terminals is not null && Terminals.Count != 0)
            {
                JsonArray jsonArray = [];
                foreach (PartFVentilationTerminalRequirement terminal in Terminals)
                {
                    JsonObject jsonObject_Terminal = terminal?.ToJsonObject();
                    if (jsonObject_Terminal is not null)
                    {
                        jsonArray.Add(jsonObject_Terminal);
                    }
                }

                result["Terminals"] = jsonArray;
            }

            JsonObject jsonObject_Purge = Purge?.ToJsonObject();
            if (jsonObject_Purge is not null)
            {
                result["Purge"] = jsonObject_Purge;
            }

            return result;
        }
    }
}
