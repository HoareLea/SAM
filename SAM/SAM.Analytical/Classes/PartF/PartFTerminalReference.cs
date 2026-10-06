// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The Approved Document F requirement one design ventilation terminal was created to realize, held
    /// as a link that survives the requirement being recalculated.
    /// <para>
    /// <b>Why a reference and not a guid.</b> <see cref="PartFCalculator"/> constructs a brand new
    /// <see cref="PartFVentilationTerminalRequirement"/> for every terminal on every run, so
    /// <see cref="RequirementGuid"/> is precise while it holds and <b>stale the moment Part F is
    /// recalculated</b>. The three fields beside it are the requirement's regulatory identity - which
    /// room, which Approved Document role, which paragraph - and they do not change when the numbers
    /// are recalculated. Together they let a design terminal be re-linked to the requirement that
    /// replaced the one it was made from, explicitly and reportably.
    /// </para>
    /// <para>
    /// <b>Why <see cref="PartFTerminalRole"/> and not <see cref="FlowClassification"/>.</b> The generic
    /// terminal is classified as Supply or Extract, which is what it physically is. Approved Document F
    /// draws a distinction the generic classification deliberately collapses: extract local to the
    /// cooking function (paragraph 1.17a) and general wet room extract (paragraph 1.17) are both
    /// physically extract terminals. Recovering the right requirement therefore needs the regulatory
    /// role, and this is the only place it is carried.
    /// </para>
    /// <para>
    /// <b>This is lineage, not behaviour.</b> Nothing reads it to decide a rate, a route or an
    /// operating condition. It is attached to a <see cref="VentilationTerminal"/> through
    /// <see cref="VentilationTerminalParameter.PartFTerminalReference"/>, the same way Approved
    /// Document F data hangs off the generic <see cref="Space"/> through
    /// <see cref="SpaceParameter.PartFSpaceData"/> - so the generic terminal has no member of a
    /// regulatory type and a terminal that realizes nothing regulatory simply carries no reference.
    /// </para>
    /// </summary>
    public class PartFTerminalReference : SAMObject
    {
        public PartFTerminalReference()
        {
        }

        public PartFTerminalReference(PartFVentilationTerminalRequirement partFVentilationTerminalRequirement)
            : base(partFVentilationTerminalRequirement?.Name)
        {
            if (partFVentilationTerminalRequirement is not null)
            {
                RequirementGuid = partFVentilationTerminalRequirement.Guid;
                SpaceGuid = partFVentilationTerminalRequirement.SpaceGuid;
                TerminalRole = partFVentilationTerminalRequirement.TerminalRole;
                SourceReference = partFVentilationTerminalRequirement.SourceReference;
            }
        }

        public PartFTerminalReference(PartFTerminalReference partFTerminalReference)
            : base(partFTerminalReference)
        {
            if (partFTerminalReference is not null)
            {
                RequirementGuid = partFTerminalReference.RequirementGuid;
                SpaceGuid = partFTerminalReference.SpaceGuid;
                TerminalRole = partFTerminalReference.TerminalRole;
                SourceReference = partFTerminalReference.SourceReference;
            }
        }

        public PartFTerminalReference(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// The requirement this terminal currently realizes. Precise, and <b>only valid until Part F is
        /// recalculated</b> - see <see cref="Matches(PartFVentilationTerminalRequirement)"/>.
        /// </summary>
        public Guid RequirementGuid { get; set; } = Guid.Empty;

        /// <summary>The space the requirement applies to. Part of the stable recovery identity.</summary>
        public Guid SpaceGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// The Approved Document F role of the requirement. Part of the stable recovery identity, and
        /// the reason this class exists rather than a bare guid.
        /// </summary>
        public PartFTerminalRole TerminalRole { get; set; } = PartFTerminalRole.Undefined;

        /// <summary>
        /// The Approved Document paragraph or table the requirement comes from. Part of the stable
        /// recovery identity: it separates two requirements that a room and a role alone could not.
        /// </summary>
        public string SourceReference { get; set; }

        /// <summary>
        /// Whether <paramref name="partFVentilationTerminalRequirement"/> is the same regulatory
        /// requirement as the one this reference was made from, ignoring the guid.
        /// <para>
        /// Deliberately not an equality operator and deliberately not guid-based: the whole point is to
        /// recognise the requirement that <i>replaced</i> the original after a recalculation.
        /// </para>
        /// </summary>
        public bool Matches(PartFVentilationTerminalRequirement partFVentilationTerminalRequirement)
        {
            if (partFVentilationTerminalRequirement is null)
            {
                return false;
            }

            if (partFVentilationTerminalRequirement.SpaceGuid != SpaceGuid)
            {
                return false;
            }

            if (partFVentilationTerminalRequirement.TerminalRole != TerminalRole)
            {
                return false;
            }

            //Both absent is a match, because a requirement with no source reference is still that room's
            //terminal of that role. Only a stated disagreement is a mismatch.
            return string.Equals(partFVentilationTerminalRequirement.SourceReference, SourceReference, StringComparison.Ordinal);
        }

        /// <summary>A one-line description of what is being pointed at, for a note or a refusal.</summary>
        public string Description()
        {
            return string.Format("the {0} requirement of space {1}{2}", Core.Query.Description(TerminalRole), SpaceGuid, string.IsNullOrWhiteSpace(SourceReference) ? string.Empty : string.Format(" ({0})", SourceReference));
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            RequirementGuid = PartFJson.Guid(jsonObject, "RequirementGuid");
            SpaceGuid = PartFJson.Guid(jsonObject, "SpaceGuid");

            if (jsonObject.ContainsKey("TerminalRole"))
            {
                TerminalRole = Core.Query.Enum<PartFTerminalRole>(PartFJson.String(jsonObject, "TerminalRole"));
            }

            SourceReference = PartFJson.String(jsonObject, "SourceReference") ?? SourceReference;

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            result["RequirementGuid"] = RequirementGuid.ToString();
            result["SpaceGuid"] = SpaceGuid.ToString();
            result["TerminalRole"] = TerminalRole.ToString();

            PartFJson.SetString(result, "SourceReference", SourceReference);

            return result;
        }
    }
}
