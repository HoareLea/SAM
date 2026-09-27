// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Globalization;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One cooled dwelling of a materialised mixed model, as <see cref="PartOMaterialisationRecord"/> records it: the
    /// unit it was materialised with, the product whose manufacturer guidance cools it, the cooling operating
    /// airflow that guidance resolves to for the dwelling's design, and a fingerprint of that guidance.
    /// <para>
    /// <b>A run artefact, never design authority.</b> It lives on the materialisation record of a materialised model
    /// only - never on the baseline or the strategy. <see cref="CoolingOperatingAirFlow_Lps"/> is the operating
    /// airflow while the cooling-stat calls (<c>Query.PartOCoolingOperatingAirFlow</c>); the design airflow stays on
    /// the terminals.
    /// </para>
    /// </summary>
    public class PartOCooledDwelling : IJSAMObject, IAnalyticalObject
    {
        public PartOCooledDwelling()
        {
        }

        public PartOCooledDwelling(Guid guid_Zone, Guid guid_AirHandlingUnit, VentilationUnitReference ventilationUnitReference, double coolingOperatingAirFlow_Lps, string fingerprint_Guidance)
        {
            ZoneGuid = guid_Zone;
            AirHandlingUnitGuid = guid_AirHandlingUnit;
            VentilationUnitReference = ventilationUnitReference is null ? null : new VentilationUnitReference(ventilationUnitReference.Manufacturer, ventilationUnitReference.Model, ventilationUnitReference.Reference);
            CoolingOperatingAirFlow_Lps = coolingOperatingAirFlow_Lps;
            Fingerprint_Guidance = fingerprint_Guidance;
        }

        public PartOCooledDwelling(PartOCooledDwelling partOCooledDwelling)
            : this(partOCooledDwelling?.ZoneGuid ?? Guid.Empty, partOCooledDwelling?.AirHandlingUnitGuid ?? Guid.Empty, partOCooledDwelling?.VentilationUnitReference, partOCooledDwelling?.CoolingOperatingAirFlow_Lps ?? double.NaN, partOCooledDwelling?.Fingerprint_Guidance)
        {
        }

        public PartOCooledDwelling(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>The cooled dwelling zone.</summary>
        public Guid ZoneGuid { get; private set; } = Guid.Empty;

        /// <summary>The materialised unit that cools it - the key the Systems route is configured by.</summary>
        public Guid AirHandlingUnitGuid { get; private set; } = Guid.Empty;

        /// <summary>The product whose manufacturer guidance is the cooling.</summary>
        public VentilationUnitReference VentilationUnitReference { get; private set; }

        /// <summary>The airflow the unit moves while cooling [l/s] - an operating airflow.</summary>
        public double CoolingOperatingAirFlow_Lps { get; private set; } = double.NaN;

        /// <summary><c>Query.PartOCoolingGuidanceFingerprint</c> of the product when the model was materialised.</summary>
        public string Fingerprint_Guidance { get; private set; }

        public bool IsValid => ZoneGuid != Guid.Empty
            && AirHandlingUnitGuid != Guid.Empty
            && VentilationUnitReference is not null
            && VentilationUnitReference.IsValid
            && !double.IsNaN(CoolingOperatingAirFlow_Lps)
            && !double.IsInfinity(CoolingOperatingAirFlow_Lps)
            && CoolingOperatingAirFlow_Lps > 0
            && !string.IsNullOrEmpty(Fingerprint_Guidance);

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject is null)
            {
                return false;
            }

            ZoneGuid = Guid.TryParse(Text(jsonObject, "ZoneGuid") ?? string.Empty, out Guid guid_Zone) ? guid_Zone : Guid.Empty;
            AirHandlingUnitGuid = Guid.TryParse(Text(jsonObject, "AirHandlingUnitGuid") ?? string.Empty, out Guid guid_AirHandlingUnit) ? guid_AirHandlingUnit : Guid.Empty;
            VentilationUnitReference = jsonObject["VentilationUnitReference"] is JsonObject jsonObject_Reference
                ? new VentilationUnitReference(Text(jsonObject_Reference, "Manufacturer"), Text(jsonObject_Reference, "Model"), Text(jsonObject_Reference, "Reference"))
                : null;
            CoolingOperatingAirFlow_Lps = jsonObject["CoolingOperatingAirFlow_Lps"] is JsonValue jsonValue && jsonValue.TryGetValue(out double value) ? value : double.NaN;
            Fingerprint_Guidance = Text(jsonObject, "Fingerprint_Guidance");

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject jsonObject_Reference = [];
            if (VentilationUnitReference?.Manufacturer is not null)
            {
                jsonObject_Reference["Manufacturer"] = VentilationUnitReference.Manufacturer;
            }

            if (VentilationUnitReference?.Model is not null)
            {
                jsonObject_Reference["Model"] = VentilationUnitReference.Model;
            }

            if (VentilationUnitReference?.Reference is not null)
            {
                jsonObject_Reference["Reference"] = VentilationUnitReference.Reference;
            }

            JsonObject jsonObject = new()
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["ZoneGuid"] = ZoneGuid.ToString("D", CultureInfo.InvariantCulture),
                ["AirHandlingUnitGuid"] = AirHandlingUnitGuid.ToString("D", CultureInfo.InvariantCulture),
                ["VentilationUnitReference"] = jsonObject_Reference,
                ["Fingerprint_Guidance"] = Fingerprint_Guidance,
            };

            if (!double.IsNaN(CoolingOperatingAirFlow_Lps) && !double.IsInfinity(CoolingOperatingAirFlow_Lps))
            {
                jsonObject["CoolingOperatingAirFlow_Lps"] = CoolingOperatingAirFlow_Lps;
            }

            return jsonObject;
        }

        private static string Text(JsonObject jsonObject, string name)
        {
            return jsonObject[name] is JsonValue jsonValue && jsonValue.TryGetValue(out string result) ? result : null;
        }
    }
}
