// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One real manufacturer product, as a catalogue describes it: what it is, where the description came
    /// from, the most air it can move, and - carried but not yet used - what it does under conditions the
    /// manufacturer published.
    /// <para>
    /// <b>This is the equipment template, the second of Approved Document O Iteration 2's four
    /// quantities.</b> They are, and stay, distinct:
    /// </para>
    /// <code>
    /// requirement   PartFSpaceData.Terminals[].ContinuousDesignFlowRate_Lps   what the Approved Document demands
    /// template      VentilationUnitTemplate (this)                            what a product is capable of
    /// design        VentilationTerminal.DesignFlowRate_Lps                    what this dwelling is designed to move
    /// operating     Iteration 3, hourly                                       what it moves at 3pm in August
    /// </code>
    /// <para>
    /// <b>A template bounds a design; it never becomes one.</b> Selecting this product for an
    /// <see cref="AirHandlingUnit"/> stores its <i>identity</i> - see <see cref="VentilationUnitReference"/>
    /// - and touches no design airflow. A unit whose template is rated at 150 l/s and whose dwelling is
    /// designed for 115 l/s is a 115 l/s dwelling with 35 l/s of headroom, and the headroom is a number to
    /// report, not a number to spend.
    /// </para>
    /// <para>
    /// <b>An unresolved capacity is a state, not a zero and not a guess.</b>
    /// <see cref="MaximumSupplyFlowRate_Lps"/> and <see cref="MaximumExtractFlowRate_Lps"/> are
    /// <see cref="double.NaN"/> until somebody establishes them from a source that actually states them.
    /// A template in that state carries its full published performance data and is simply not a
    /// <i>selectable</i> product: <c>Query.CapacityDescriptor</c> returns nothing for it and says why.
    /// The alternative - reading the largest airflow in a performance table as the unit's maximum - is the
    /// specific mistake this design exists to prevent. A performance table's airflow axis lists the duty
    /// points a manufacturer chose to publish at; it is not a statement about the fan.
    /// </para>
    /// <para>
    /// <b>Supply and extract are separate fields on purpose</b>, matching
    /// <see cref="VentilationUnitCapacityDescriptor"/>: a product whose two sides are differently rated is
    /// ordinary, and one figure covering both would quietly approve equipment for a duty it cannot move on
    /// one side.
    /// </para>
    /// </summary>
    public class VentilationUnitTemplate : SAMObject
    {
        public VentilationUnitTemplate()
        {
        }

        public VentilationUnitTemplate(VentilationUnitReference ventilationUnitReference, string source)
            : base(ventilationUnitReference is null ? null : ventilationUnitReference.ToString())
        {
            //Copied: the reference is mutable, and a template's identity must not change underneath a
            //catalogue that has already been validated against it.
            VentilationUnitReference = ventilationUnitReference is null ? null : new VentilationUnitReference(ventilationUnitReference);
            Source = source;
        }

        public VentilationUnitTemplate(VentilationUnitTemplate ventilationUnitTemplate)
            : base(ventilationUnitTemplate)
        {
            if (ventilationUnitTemplate is not null)
            {
                VentilationUnitReference = ventilationUnitTemplate.VentilationUnitReference is null ? null : new VentilationUnitReference(ventilationUnitTemplate.VentilationUnitReference);
                CoolingModuleModel = ventilationUnitTemplate.CoolingModuleModel;
                Source = ventilationUnitTemplate.Source;
                MaximumSupplyFlowRate_Lps = ventilationUnitTemplate.MaximumSupplyFlowRate_Lps;
                MaximumExtractFlowRate_Lps = ventilationUnitTemplate.MaximumExtractFlowRate_Lps;
                UnresolvedCapacityNote = ventilationUnitTemplate.UnresolvedCapacityNote;
                Rank = ventilationUnitTemplate.Rank;
                PerformanceTable = ventilationUnitTemplate.PerformanceTable is null ? null : new VentilationUnitPerformanceTable(ventilationUnitTemplate.PerformanceTable);
                FlowFractionByControlTemperature = ventilationUnitTemplate.FlowFractionByControlTemperature is null ? null : new FlowFractionControlCurve(ventilationUnitTemplate.FlowFractionByControlTemperature);
                HeatRecoveryPerformance = ventilationUnitTemplate.HeatRecoveryPerformance is null ? null : new HeatRecoveryPerformance(ventilationUnitTemplate.HeatRecoveryPerformance);
                FanPerformance = ventilationUnitTemplate.FanPerformance is null ? null : new FanPerformance(ventilationUnitTemplate.FanPerformance);
                OperatingStrategy = ventilationUnitTemplate.OperatingStrategy is null ? null : new VentilationUnitOperatingStrategy(ventilationUnitTemplate.OperatingStrategy);
            }
        }

        public VentilationUnitTemplate(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// The product's identity - manufacturer, model, and a reference distinguishing variants. This is
        /// the only part of a template that is ever stored on a model.
        /// </summary>
        public VentilationUnitReference VentilationUnitReference { get; set; }

        /// <summary>
        /// The cooling module or other accessory this template's performance was published <i>with</i>, where
        /// the product is a combination - "MR-ECO-COOL-V" on a Nuaire hybrid unit.
        /// <para>
        /// Held as its own field as well as inside
        /// <see cref="Analytical.VentilationUnitReference.Reference"/>, because a combination is genuinely a
        /// different product for selection purposes - the same base unit with and without a cooling module
        /// publishes different performance and has to be two catalogue entries - and a schedule needs to be
        /// able to print the accessory's model number rather than parse it out of an identity string.
        /// </para>
        /// </summary>
        public string CoolingModuleModel { get; set; }

        /// <summary>
        /// Where every figure on this template came from: the document, its issue, its date.
        /// <para>
        /// <b>Required, not decorative.</b> A template is a transcription of somebody's published data, and
        /// a transcription nobody can trace back is not manufacturer data - it is a number in a file. Every
        /// figure here is eventually going into a compliance assessment, so <see cref="IsValid"/> refuses a
        /// template without a source.
        /// </para>
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The most supply air [l/s] the unit can move, or <see cref="double.NaN"/> where no source
        /// establishes it.
        /// <para>
        /// <b>NaN means unresolved and nothing else.</b> Not zero, not "assume the biggest number in the
        /// performance table", not "assume the same as extract". A product whose capacity nobody has stated
        /// is simply not selectable until somebody states it - see <see cref="UnresolvedCapacityNote"/>.
        /// </para>
        /// <para>
        /// Litres per second, following the unit every airflow in the Approved Document F and O work is
        /// expressed in - <c>VentilationTerminal.DesignFlowRate_Lps</c>,
        /// <c>PartFSpaceData.Terminals[].ContinuousDesignFlowRate_Lps</c>,
        /// <see cref="VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps"/>. A template in cubic
        /// metres per second would need converting at exactly the seam where a units mistake is least
        /// visible.
        /// </para>
        /// </summary>
        public double MaximumSupplyFlowRate_Lps { get; set; } = double.NaN;

        /// <summary>
        /// The most extract air [l/s] the unit can move, or <see cref="double.NaN"/> where no source
        /// establishes it. Independent of <see cref="MaximumSupplyFlowRate_Lps"/> - see the type remarks.
        /// </summary>
        public double MaximumExtractFlowRate_Lps { get; set; } = double.NaN;

        /// <summary>
        /// Why the capacities are unresolved, and what would resolve them - carried so that a refusal can
        /// say something an engineer can act on instead of "no capacity".
        /// <para>
        /// Ignored entirely once the capacities are stated. It is a note about an absence.
        /// </para>
        /// </summary>
        public string UnresolvedCapacityNote { get; set; }

        /// <summary>
        /// Where this product sits in the preference order of whoever supplied the catalogue. Lower is
        /// preferred, and it is only ever a tie-break - see
        /// <see cref="VentilationUnitCapacityDescriptor.Rank"/>, which this is carried through to.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Everything the manufacturer published about how the unit performs, exactly as published.
        /// <para>
        /// <b>Dormant in Iteration 2.</b> No part of the selection or sizing path reads it. It is here so
        /// that Iteration 3 - which has to produce an hourly leaving-air temperature and airflow - starts
        /// from the manufacturer's own numbers, and so that adding it later would not have meant reopening
        /// the catalogue format.
        /// </para>
        /// </summary>
        public VentilationUnitPerformanceTable PerformanceTable { get; set; }

        /// <summary>
        /// How the product's controller varies airflow with temperature.
        /// <para>
        /// <b>Dormant in Iteration 2</b>, and template data rather than an algorithm - see
        /// <see cref="FlowFractionControlCurve"/>. Which temperature drives it, where one unit serves
        /// several rooms, is an Iteration 3 control question that nothing here answers.
        /// </para>
        /// </summary>
        public FlowFractionControlCurve FlowFractionByControlTemperature { get; set; }

        /// <summary>
        /// The product's certified sensible heat recovery efficiency against airflow, or null where nobody
        /// has transcribed it.
        /// <para>
        /// <b>Null means not stated, and nothing else</b> - not a zero efficiency, and not the efficiency a
        /// system template happens to ship with. <c>Query.VentilationUnitOperatingParameters</c> refuses to
        /// resolve heat recovery for a template without it. Optional: a product catalogued for selection
        /// alone carries none, and a catalogue written before this field existed reads exactly as it did.
        /// </para>
        /// </summary>
        public HeatRecoveryPerformance HeatRecoveryPerformance { get; set; }

        /// <summary>
        /// The product's certified specific fan power against airflow, or null where nobody has transcribed
        /// it. Null means not stated - see <see cref="HeatRecoveryPerformance"/>; electrical figures quoted
        /// in words in <see cref="Source"/> are not this.
        /// </summary>
        public FanPerformance FanPerformance { get; set; }

        /// <summary>
        /// The operating strategy the manufacturer states for representing this product's control behaviour
        /// in a dynamic thermal model, or null where nobody has transcribed one.
        /// <para>
        /// <b>Null means not stated, and nothing else.</b> A template without one is exactly the template it
        /// always was; no default strategy is assumed, and no other product's strategy is substituted.
        /// </para>
        /// <para>
        /// <b>Manufacturer modelling guidance, never certified performance.</b> A strategy states how the
        /// unit is recommended to be operated in a model. It is not <see cref="HeatRecoveryPerformance"/> and
        /// not <see cref="FanPerformance"/>, it satisfies neither, and carrying one says nothing about
        /// whether either is available - see <see cref="VentilationUnitOperatingStrategy"/>.
        /// </para>
        /// </summary>
        public VentilationUnitOperatingStrategy OperatingStrategy { get; set; }

        /// <summary>
        /// Whether this is a template at all: it names a product and it says where its figures came from.
        /// <para>
        /// Deliberately <b>not</b> a statement about capacity. A template with full published performance
        /// data and no established maximum airflow is a perfectly good record of a product; it is just not
        /// one a selection can use - <see cref="HasSelectionCapacity"/> is that question, asked separately.
        /// </para>
        /// </summary>
        public bool IsValid
        {
            get
            {
                return VentilationUnitReference is not null && VentilationUnitReference.IsValid && !string.IsNullOrWhiteSpace(Source);
            }
        }

        /// <summary>
        /// Whether the template states a capacity a selection can be made against - both sides, each a
        /// finite non-negative number of litres per second.
        /// <para>
        /// Both sides, because <see cref="VentilationUnitCapacityDescriptor"/> checks both independently
        /// and a half-stated capacity would let one side be compared against nothing.
        /// </para>
        /// </summary>
        public bool HasSelectionCapacity
        {
            get
            {
                return IsValid && IsUsableCapacity(MaximumSupplyFlowRate_Lps) && IsUsableCapacity(MaximumExtractFlowRate_Lps);
            }
        }

        /// <summary>
        /// Why this template cannot be offered to a selection, in words - or null where it can be.
        /// <para>
        /// Names the product, says which side is missing, and repeats
        /// <see cref="UnresolvedCapacityNote"/> where the catalogue supplied one, so the sentence that
        /// reaches a report is actionable rather than merely negative.
        /// </para>
        /// </summary>
        public string SelectionCapacityRefusal
        {
            get
            {
                if (HasSelectionCapacity)
                {
                    return null;
                }

                if (!IsValid)
                {
                    return VentilationUnitReference is null || !VentilationUnitReference.IsValid
                        ? "A ventilation unit template that names no product cannot be offered to a selection."
                        : string.Format("The ventilation unit template '{0}' states no source, so its figures cannot be traced to a published document and it is not offered to a selection.", VentilationUnitReference);
                }

                string sides = !IsUsableCapacity(MaximumSupplyFlowRate_Lps) && !IsUsableCapacity(MaximumExtractFlowRate_Lps)
                    ? "maximum supply and maximum extract airflow"
                    : (!IsUsableCapacity(MaximumSupplyFlowRate_Lps) ? "maximum supply airflow" : "maximum extract airflow");

                string result = string.Format(
                    "The ventilation unit template '{0}' does not establish its {1}, so it cannot be offered to a selection. A capacity has to come from a source that states it - the largest airflow in a performance table is a published duty point, not the unit's maximum.",
                    VentilationUnitReference,
                    sides);

                return string.IsNullOrWhiteSpace(UnresolvedCapacityNote) ? result : string.Format("{0} {1}", result, UnresolvedCapacityNote);
            }
        }

        public override string ToString()
        {
            string capacity = HasSelectionCapacity
                ? string.Format("supply {0:0.###} l/s, extract {1:0.###} l/s", MaximumSupplyFlowRate_Lps, MaximumExtractFlowRate_Lps)
                : "capacity unresolved";

            return string.Format("{0} [{1}]", VentilationUnitReference is null ? "-" : VentilationUnitReference.ToString(), capacity);
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            VentilationUnitReference = jsonObject["VentilationUnitReference"] is JsonObject jsonObject_VentilationUnitReference ? new VentilationUnitReference(jsonObject_VentilationUnitReference) : null;
            CoolingModuleModel = PerformanceJson.Text(jsonObject, "CoolingModuleModel");
            Source = PerformanceJson.Text(jsonObject, "Source");
            MaximumSupplyFlowRate_Lps = PerformanceJson.Value(jsonObject, "MaximumSupplyFlowRate_Lps");
            MaximumExtractFlowRate_Lps = PerformanceJson.Value(jsonObject, "MaximumExtractFlowRate_Lps");
            UnresolvedCapacityNote = PerformanceJson.Text(jsonObject, "UnresolvedCapacityNote");
            Rank = PerformanceJson.Integer(jsonObject, "Rank", 0);
            PerformanceTable = jsonObject["PerformanceTable"] is JsonObject jsonObject_PerformanceTable ? new VentilationUnitPerformanceTable(jsonObject_PerformanceTable) : null;
            FlowFractionByControlTemperature = jsonObject["FlowFractionByControlTemperature"] is JsonObject jsonObject_FlowFractionByControlTemperature ? new FlowFractionControlCurve(jsonObject_FlowFractionByControlTemperature) : null;

            //Absent keys - every catalogue written before these fields existed - read as null, "not stated".
            HeatRecoveryPerformance = jsonObject["HeatRecoveryPerformance"] is JsonObject jsonObject_HeatRecoveryPerformance ? new HeatRecoveryPerformance(jsonObject_HeatRecoveryPerformance) : null;
            FanPerformance = jsonObject["FanPerformance"] is JsonObject jsonObject_FanPerformance ? new FanPerformance(jsonObject_FanPerformance) : null;
            OperatingStrategy = jsonObject["OperatingStrategy"] is JsonObject jsonObject_OperatingStrategy ? new VentilationUnitOperatingStrategy(jsonObject_OperatingStrategy) : null;

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            if (VentilationUnitReference is not null)
            {
                result["VentilationUnitReference"] = VentilationUnitReference.ToJsonObject();
            }

            PerformanceJson.SetText(result, "CoolingModuleModel", CoolingModuleModel);
            PerformanceJson.SetText(result, "Source", Source);

            //Omitted rather than written as null where unresolved - see PerformanceJson.Value, which reads
            //absent and unusable back as the same NaN, so an unresolved capacity survives a round trip as
            //exactly one state.
            PerformanceJson.SetValue(result, "MaximumSupplyFlowRate_Lps", MaximumSupplyFlowRate_Lps);
            PerformanceJson.SetValue(result, "MaximumExtractFlowRate_Lps", MaximumExtractFlowRate_Lps);

            PerformanceJson.SetText(result, "UnresolvedCapacityNote", UnresolvedCapacityNote);

            result["Rank"] = Rank;

            if (PerformanceTable is not null)
            {
                result["PerformanceTable"] = PerformanceTable.ToJsonObject();
            }

            if (FlowFractionByControlTemperature is not null)
            {
                result["FlowFractionByControlTemperature"] = FlowFractionByControlTemperature.ToJsonObject();
            }

            //Omitted where not stated, so a template without them serializes exactly as it always did.
            if (HeatRecoveryPerformance is not null)
            {
                result["HeatRecoveryPerformance"] = HeatRecoveryPerformance.ToJsonObject();
            }

            if (FanPerformance is not null)
            {
                result["FanPerformance"] = FanPerformance.ToJsonObject();
            }

            if (OperatingStrategy is not null)
            {
                result["OperatingStrategy"] = OperatingStrategy.ToJsonObject();
            }

            return result;
        }

        private static bool IsUsableCapacity(double value_Lps)
        {
            return !double.IsNaN(value_Lps) && !double.IsInfinity(value_Lps) && value_Lps >= 0;
        }
    }
}
