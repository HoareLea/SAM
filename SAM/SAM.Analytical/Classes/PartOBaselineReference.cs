// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// What a saved Part O result was derived from, stamped on the result model as
    /// <c>AnalyticalModelParameter.PartOBaselineReference</c> - the durable "derived from" of the model-state
    /// architecture (PR-5).
    ///
    /// <para><b>Why it exists</b></para>
    /// <para>
    /// A result model keeps its design's guid and, for 1a, 1b and 2, its name, and its only link to a design was a
    /// content hash. So a reopened result could not say which design it came from, what case it was, or whether that
    /// design had changed since. This is that link, carried <b>on the result</b>, so reopening a result needs neither
    /// the model that happens to be open nor a file name.
    /// </para>
    ///
    /// <para><b>Two references at most</b></para>
    /// <list type="bullet">
    /// <item><see cref="Design"/> - the design model the whole lineage derives from. Present whenever it is known:
    /// Iteration 1a, 1b, 2 and Mixed Design record it directly, and Iteration 2B and Iteration 3 inherit it from the
    /// result they derive from.</item>
    /// <item><see cref="Source"/> - the immediate source <b>result</b>, for the two cases that derive from a result:
    /// Iteration 2B (the Iteration 2 result) and Iteration 3 (the 1a or 2 result it is paired with). Null for a case
    /// derived from the design model directly.</item>
    /// </list>
    ///
    /// <para><b>Not a design baseline, not an authority</b></para>
    /// <para>
    /// Carrying the reference is what marks a model as a result: <c>Query.PartOBaselineFindings</c> refuses it as a
    /// baseline and <c>Modify.RemovePartORunState</c> removes it. Opening a result never makes it, or what it
    /// references, the open design model. Nothing here decides a calculation.
    /// </para>
    ///
    /// <para><b>Absent means unknown, and a later schema is not reinterpreted</b></para>
    /// <para>
    /// A result saved before this existed has no reference; nothing is inferred from its name, folder or fingerprints.
    /// A reference of a schema this build does not know loads as <see cref="IsValid"/> false and round-trips as read.
    /// </para>
    /// </summary>
    public class PartOBaselineReference : IJSAMObject, IAnalyticalObject
    {
        /// <summary>The persisted schema this build reads and writes.</summary>
        public const string Schema = "PartOBaselineReference:v1";

        public PartOBaselineReference()
        {
        }

        public PartOBaselineReference(PartODerivedCase @case, PartOModelReference design, PartOModelReference source)
        {
            Case = @case;
            Design = design is null ? null : new PartOModelReference(design);
            Source = source is null ? null : new PartOModelReference(source);
        }

        public PartOBaselineReference(PartOBaselineReference partOBaselineReference)
        {
            if (partOBaselineReference is not null)
            {
                SchemaRead = partOBaselineReference.SchemaRead;
                Case = partOBaselineReference.Case;
                Design = partOBaselineReference.Design is null ? null : new PartOModelReference(partOBaselineReference.Design);
                Source = partOBaselineReference.Source is null ? null : new PartOModelReference(partOBaselineReference.Source);
            }
        }

        public PartOBaselineReference(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>The schema the reference was read with - <see cref="Schema"/> unless it was loaded from something else.</summary>
        public string SchemaRead { get; private set; } = Schema;

        /// <summary>The case the result is.</summary>
        public PartODerivedCase Case { get; set; } = PartODerivedCase.Undefined;

        /// <summary>The design model the lineage derives from, or null where it is not known (a result derived from a legacy result).</summary>
        public PartOModelReference Design { get; set; }

        /// <summary>The immediate source result (Iteration 2B, Iteration 3), or null where the case derives from the design directly.</summary>
        public PartOModelReference Source { get; set; }

        /// <summary>Whether the case derives from a result (2B, 3) rather than from the design model directly.</summary>
        public bool DerivesFromResult => Case == PartODerivedCase.Iteration2B || Case == PartODerivedCase.Iteration3;

        /// <summary>
        /// A schema this build knows, a stated case, and the right reference for it: a valid source result for the
        /// cases derived from a result, a valid design for the cases derived from the design. A design, where stated,
        /// must be valid and of the design kind.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (SchemaRead != Schema || Case == PartODerivedCase.Undefined)
                {
                    return false;
                }

                if (Design is not null && (!Design.IsValid || Design.Kind != PartOModelReferenceKind.Design))
                {
                    return false;
                }

                if (DerivesFromResult)
                {
                    return Source is not null && Source.IsValid && Source.Kind == PartOModelReferenceKind.Result;
                }

                return Source is null && Design is not null;
            }
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject is null)
            {
                return false;
            }

            SchemaRead = PartOModelReference.Text(jsonObject, "Schema");
            Case = Enum.TryParse(PartOModelReference.Text(jsonObject, "Case"), out PartODerivedCase @case) ? @case : PartODerivedCase.Undefined;
            Design = jsonObject["Design"] is JsonObject jsonObject_Design ? new PartOModelReference(jsonObject_Design) : null;
            Source = jsonObject["Source"] is JsonObject jsonObject_Source ? new PartOModelReference(jsonObject_Source) : null;

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject result = new()
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["Schema"] = SchemaRead ?? Schema,
                ["Case"] = Case.ToString(),
            };

            if (Design is not null)
            {
                result["Design"] = Design.ToJsonObject();
            }

            if (Source is not null)
            {
                result["Source"] = Source.ToJsonObject();
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} derived from {1}", Case, Source is not null ? Source : Design);
        }
    }
}
