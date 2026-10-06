// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// What a system has to be able to do for a given dwelling - the engineering requirement, stated
    /// independently of which systems exist.
    /// <para>
    /// This is the analytical half of choosing a system, and it is the whole of what crosses the boundary.
    /// <c>SAM.Analytical</c> says what is needed; <c>SAM_Systems</c> owns which of its shipped
    /// <c>SystemEnergyCentre</c> templates provide it. Neither knows the other's business, which is the
    /// same cut the handover already makes for TAS: the calculation states intent, the specialised
    /// assembly owns the implementation.
    /// </para>
    /// <para>
    /// <b>A requirement is not an identity.</b> It takes no part in <c>OverheatingScenario.Key</c> - it is
    /// derived from a Part F assessment, and two dwellings needing the same things are still two
    /// assessments.
    /// </para>
    /// </summary>
    public class SystemCapabilityRequirement : IJSAMObject, IAnalyticalObject
    {
        /// <summary>
        /// The order capabilities are written and read in. Explicit rather than
        /// <c>Enum.GetValues</c> so that adding a member cannot silently reorder an existing file.
        /// </summary>
        private static readonly SystemCapability[] systemCapabilities =
        [
            SystemCapability.ContinuousVentilation,
            SystemCapability.MechanicalSupply,
            SystemCapability.Boost,
            SystemCapability.SummerBypass,
            SystemCapability.HeatRecovery
        ];

        private SystemCapability systemCapability = SystemCapability.None;

        public SystemCapabilityRequirement()
        {

        }

        public SystemCapabilityRequirement(SystemCapability systemCapability)
        {
            this.systemCapability = systemCapability;
        }

        public SystemCapabilityRequirement(SystemCapabilityRequirement systemCapabilityRequirement)
        {
            if (systemCapabilityRequirement != null)
            {
                systemCapability = systemCapabilityRequirement.systemCapability;
            }
        }

        public SystemCapabilityRequirement(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>Everything the system must be able to do.</summary>
        public SystemCapability Capabilities => systemCapability;

        /// <summary>Whether a particular capability is required.</summary>
        public bool Requires(SystemCapability systemCapability)
        {
            return systemCapability != SystemCapability.None && (this.systemCapability & systemCapability) == systemCapability;
        }

        /// <summary>
        /// The same requirement with more asked of it. Returns a new instance - a requirement handed to a
        /// selector must not change underneath it.
        /// </summary>
        public SystemCapabilityRequirement With(SystemCapability systemCapability)
        {
            return new SystemCapabilityRequirement(this.systemCapability | systemCapability);
        }

        /// <summary>Whether anything is required at all.</summary>
        public bool IsValid => systemCapability != SystemCapability.None;

        /// <summary>
        /// The capabilities required but not provided by the given ones - the reason a system was
        /// refused, in the same vocabulary the requirement was stated in.
        /// </summary>
        public SystemCapability Missing(SystemCapability systemCapability)
        {
            return this.systemCapability & ~systemCapability;
        }

        /// <summary>Whether the given capabilities meet this requirement in full.</summary>
        public bool IsMetBy(SystemCapability systemCapability)
        {
            return Missing(systemCapability) == SystemCapability.None;
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
            {
                return false;
            }

            systemCapability = SystemCapability.None;

            if (!(jsonObject["Capabilities"] is JsonArray jsonArray))
            {
                return false;
            }

            //Read by name against the members this build has, so an unknown capability from a later
            //version is ignored rather than turned into a bit nothing here understands.
            foreach (JsonNode jsonNode in jsonArray)
            {
                if (!(jsonNode is JsonValue jsonValue) || !jsonValue.TryGetValue(out string text))
                {
                    continue;
                }

                foreach (SystemCapability systemCapability_Temp in systemCapabilities)
                {
                    if (string.Equals(text, systemCapability_Temp.ToString(), StringComparison.Ordinal))
                    {
                        systemCapability |= systemCapability_Temp;
                        break;
                    }
                }
            }

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject jsonObject = new()
            {
                ["_type"] = Core.Query.FullTypeName(this)
            };

            JsonArray jsonArray = [];

            foreach (SystemCapability systemCapability_Temp in systemCapabilities)
            {
                if (Requires(systemCapability_Temp))
                {
                    jsonArray.Add(systemCapability_Temp.ToString());
                }
            }

            jsonObject["Capabilities"] = jsonArray;

            return jsonObject;
        }

        public override string ToString()
        {
            List<string> strings = [];

            foreach (SystemCapability systemCapability_Temp in systemCapabilities)
            {
                if (Requires(systemCapability_Temp))
                {
                    strings.Add(systemCapability_Temp.ToString());
                }
            }

            return strings.Count == 0 ? SystemCapability.None.ToString() : string.Join(", ", strings);
        }
    }
}
