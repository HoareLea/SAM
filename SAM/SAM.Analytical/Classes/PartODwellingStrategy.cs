// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The Approved Document O strategy an engineer has <b>selected</b> for one dwelling - the persisted
    /// intent a mixed model is materialised from (<c>Modify.MaterialisePartODwellingStrategies</c>).
    ///
    /// <para><b>Intent only, and orthogonal</b></para>
    /// <para>
    /// Four independent properties rather than one enum of combinations: the ventilation route, the product
    /// (or none, meaning "select from the project pool"), active cooling, and the design airflow basis.
    /// "Optimised MVHR" is MVHR + <see cref="PartODesignAirFlowBasis.RetainedDesign"/>; "MVHR + cooling" is
    /// MVHR + <see cref="PartOActiveCooling.SupplyAirCooling"/>. Contradictory combinations (natural
    /// ventilation with a product, a retained mechanical design or cooling) are representable so that they can
    /// be refused by name, never silently resolved.
    /// </para>
    ///
    /// <para><b>It holds no engineering numbers</b></para>
    /// <para>
    /// No flow, no capacity, no operating airflow. The design airflow stays on
    /// <c>VentilationTerminal.DesignFlowRate_Lps</c>; the capability stays on the catalogue; the operating
    /// airflow stays on the operating strategy. For a retained design the strategy holds only a fingerprint of
    /// the baseline terminal set the engineer accepted (<see cref="DesignFingerprint"/>), which guards that set
    /// and cannot be read back as a flow. So <c>PartFRequiredAirFlow != DesignAirFlow !=
    /// SelectedEquipmentCapacity != OperatingAirFlow</c> is unchanged.
    /// </para>
    ///
    /// <para><b>Selected, never suggested</b></para>
    /// <para>
    /// There is no "suggested" state here. A dwelling with no strategy is a dwelling nobody has decided about
    /// - it is never read as natural ventilation, and the absence of a mechanical system is never read as a
    /// selection either. Project-wide rules (all dwellings MVHR, the permitted product pool) are project
    /// settings (<see cref="PartOEquipmentSelection"/>) and are not copied into each strategy.
    /// </para>
    ///
    /// <para><b>No instance identity</b></para>
    /// <para>
    /// Deliberately not a <see cref="SAMObject"/>: a strategy has no guid of its own, so two logically
    /// identical strategy sets serialise and fingerprint identically. The dwelling it belongs to is
    /// <see cref="ZoneGuid"/>. The product is persisted by its identity fields alone, for the same reason.
    /// </para>
    /// </summary>
    public class PartODwellingStrategy : IJSAMObject, IAnalyticalObject
    {
        public PartODwellingStrategy()
        {
        }

        /// <param name="guid_Zone">The dwelling zone.</param>
        /// <param name="partOVentilationMode">The selected ventilation route.</param>
        /// <param name="ventilationUnitReference">The selected product, or null for "select from the project pool".</param>
        /// <param name="partOActiveCooling">Active cooling. Gated - see <see cref="PartOActiveCooling"/>.</param>
        /// <param name="partODesignAirFlowBasis">Which design airflow to materialise at.</param>
        /// <param name="designFingerprint">
        /// For <see cref="PartODesignAirFlowBasis.RetainedDesign"/>, the fingerprint of the accepted terminal set
        /// (<c>Query.PartODwellingDesignFingerprint</c>). Ignored otherwise.
        /// </param>
        public PartODwellingStrategy(Guid guid_Zone, PartOVentilationMode partOVentilationMode, VentilationUnitReference ventilationUnitReference = null, PartOActiveCooling partOActiveCooling = PartOActiveCooling.None, PartODesignAirFlowBasis partODesignAirFlowBasis = PartODesignAirFlowBasis.PartFRequirement, string designFingerprint = null)
        {
            ZoneGuid = guid_Zone;
            VentilationMode = partOVentilationMode;
            VentilationUnitReference = ventilationUnitReference;
            ActiveCooling = partOActiveCooling;
            DesignAirFlowBasis = partODesignAirFlowBasis;
            DesignFingerprint = designFingerprint;
        }

        public PartODwellingStrategy(PartODwellingStrategy partODwellingStrategy)
        {
            if (partODwellingStrategy is not null)
            {
                ZoneGuid = partODwellingStrategy.ZoneGuid;
                VentilationMode = partODwellingStrategy.VentilationMode;
                VentilationUnitReference = partODwellingStrategy.VentilationUnitReference;
                ActiveCooling = partODwellingStrategy.ActiveCooling;
                DesignAirFlowBasis = partODwellingStrategy.DesignAirFlowBasis;
                DesignFingerprint = partODwellingStrategy.DesignFingerprint;
                ventilationUnitReference_Unreadable = partODwellingStrategy.ventilationUnitReference_Unreadable;
            }
        }

        public PartODwellingStrategy(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        private VentilationUnitReference ventilationUnitReference;

        //A product was stated but identifies nothing. Kept as its own state, never collapsed to null: null means
        //"select from the project pool", so collapsing would silently turn a manual choice into an automatic one.
        private bool ventilationUnitReference_Unreadable;

        /// <summary>The dwelling zone this strategy is selected for.</summary>
        public Guid ZoneGuid { get; set; } = Guid.Empty;

        /// <summary>The selected ventilation route. <see cref="PartOVentilationMode.Undefined"/> is invalid, never a default.</summary>
        public PartOVentilationMode VentilationMode { get; set; } = PartOVentilationMode.Undefined;

        /// <summary>
        /// The selected ventilation unit product, by identity, as a copy; null means "select from the project
        /// pool". Never carries a capacity - see <see cref="Analytical.VentilationUnitReference"/>.
        /// </summary>
        public VentilationUnitReference VentilationUnitReference
        {
            get => ventilationUnitReference is null ? null : new VentilationUnitReference(ventilationUnitReference.Manufacturer, ventilationUnitReference.Model, ventilationUnitReference.Reference);
            set
            {
                ventilationUnitReference = value is null || !value.IsValid ? null : new VentilationUnitReference(value.Manufacturer, value.Model, value.Reference);
                ventilationUnitReference_Unreadable = value is not null && !value.IsValid;
            }
        }

        /// <summary>Active cooling. Recorded, and refused by PR1 materialisation.</summary>
        public PartOActiveCooling ActiveCooling { get; set; } = PartOActiveCooling.Undefined;

        /// <summary>Which design airflow the dwelling is materialised at.</summary>
        public PartODesignAirFlowBasis DesignAirFlowBasis { get; set; } = PartODesignAirFlowBasis.Undefined;

        /// <summary>
        /// The accepted terminal set's fingerprint, for <see cref="PartODesignAirFlowBasis.RetainedDesign"/>.
        /// A guard, not a store: it cannot be read back as an airflow.
        /// </summary>
        public string DesignFingerprint { get; set; }

        /// <summary>
        /// Every property is stated. Contradictions (natural ventilation with a product, cooling or a retained
        /// design) are NOT invalid here - they are refused by the materialisation with their own reasons.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (ZoneGuid == Guid.Empty || VentilationMode == PartOVentilationMode.Undefined || ActiveCooling == PartOActiveCooling.Undefined || DesignAirFlowBasis == PartODesignAirFlowBasis.Undefined || ventilationUnitReference_Unreadable)
                {
                    return false;
                }

                return DesignAirFlowBasis != PartODesignAirFlowBasis.RetainedDesign || !string.IsNullOrWhiteSpace(DesignFingerprint);
            }
        }

        /// <summary>
        /// The strategy's identity-defining state as one line of invariant text: the input to the strategy-set
        /// fingerprint of a materialisation record. The product is compared by its identity fields, never by a
        /// guid, so a re-created reference to the same product is the same strategy.
        /// </summary>
        public string CanonicalText()
        {
            return string.Join("|",
                ZoneGuid.ToString("D", CultureInfo.InvariantCulture),
                VentilationMode.ToString(),
                ventilationUnitReference_Unreadable ? "unreadable" : ventilationUnitReference is null ? "-" : string.Join("/", Text(ventilationUnitReference.Manufacturer), Text(ventilationUnitReference.Model), Text(ventilationUnitReference.Reference)),
                ActiveCooling.ToString(),
                DesignAirFlowBasis.ToString(),
                DesignAirFlowBasis == PartODesignAirFlowBasis.RetainedDesign ? Text(DesignFingerprint) : "-");
        }

        public override string ToString()
        {
            List<string> parts = [Core.Query.Description(VentilationMode)];

            if (ventilationUnitReference is not null)
            {
                parts.Add(ventilationUnitReference.ToString());
            }

            if (ActiveCooling == PartOActiveCooling.SupplyAirCooling)
            {
                parts.Add(Core.Query.Description(ActiveCooling));
            }

            if (DesignAirFlowBasis == PartODesignAirFlowBasis.RetainedDesign)
            {
                parts.Add(Core.Query.Description(DesignAirFlowBasis));
            }

            return string.Join(", ", parts);
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject is null)
            {
                return false;
            }

            //By name, and an unknown name is Undefined - which makes the strategy invalid and refused - rather
            //than an exception or, worse, the first member. A strategy written by a later version must not be
            //read as a different decision.
            ZoneGuid = Guid.TryParse(Text(jsonObject, "ZoneGuid") ?? string.Empty, out Guid guid) ? guid : Guid.Empty;
            VentilationMode = Parse(Text(jsonObject, "VentilationMode"), PartOVentilationMode.Undefined);
            ActiveCooling = Parse(Text(jsonObject, "ActiveCooling"), PartOActiveCooling.Undefined);
            DesignAirFlowBasis = Parse(Text(jsonObject, "DesignAirFlowBasis"), PartODesignAirFlowBasis.Undefined);
            DesignFingerprint = Text(jsonObject, "DesignFingerprint");

            ventilationUnitReference = null;
            ventilationUnitReference_Unreadable = false;
            if (jsonObject.ContainsKey("VentilationUnitReference"))
            {
                //Present but not an identity (not an object, or naming nothing) is unreadable, which invalidates the
                //strategy - never "no product".
                VentilationUnitReference = jsonObject["VentilationUnitReference"] is JsonObject jsonObject_Reference
                    ? new VentilationUnitReference(Text(jsonObject_Reference, "Manufacturer"), Text(jsonObject_Reference, "Model"), Text(jsonObject_Reference, "Reference"))
                    : new VentilationUnitReference();
            }

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject jsonObject = new()
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["ZoneGuid"] = ZoneGuid.ToString("D", CultureInfo.InvariantCulture),
                ["VentilationMode"] = VentilationMode.ToString(),
                ["ActiveCooling"] = ActiveCooling.ToString(),
                ["DesignAirFlowBasis"] = DesignAirFlowBasis.ToString(),
            };

            //The product's identity fields only - no guid, no type tag - so the same selection always writes the
            //same bytes, and the model fingerprint of a baseline does not move when a picker re-creates it.
            if (ventilationUnitReference_Unreadable)
            {
                //Written back as unreadable, so re-saving an invalid strategy cannot turn it into "no product".
                jsonObject["VentilationUnitReference"] = new JsonObject();
            }
            else if (ventilationUnitReference is not null)
            {
                JsonObject jsonObject_Reference = [];

                if (ventilationUnitReference.Manufacturer is not null)
                {
                    jsonObject_Reference["Manufacturer"] = ventilationUnitReference.Manufacturer;
                }

                if (ventilationUnitReference.Model is not null)
                {
                    jsonObject_Reference["Model"] = ventilationUnitReference.Model;
                }

                if (ventilationUnitReference.Reference is not null)
                {
                    jsonObject_Reference["Reference"] = ventilationUnitReference.Reference;
                }

                jsonObject["VentilationUnitReference"] = jsonObject_Reference;
            }

            if (DesignFingerprint is not null)
            {
                jsonObject["DesignFingerprint"] = DesignFingerprint;
            }

            return jsonObject;
        }

        private static T Parse<T>(string text, T @default) where T : struct
        {
            return Enum.TryParse(text ?? string.Empty, out T value) && Enum.IsDefined(typeof(T), value) ? value : @default;
        }

        private static string Text(string text) => text ?? string.Empty;

        private static string Text(JsonObject jsonObject, string name)
        {
            if (jsonObject is null || name is null || !jsonObject.ContainsKey(name))
            {
                return null;
            }

            return jsonObject[name] is JsonValue jsonValue && jsonValue.TryGetValue(out string result) ? result : null;
        }
    }
}
