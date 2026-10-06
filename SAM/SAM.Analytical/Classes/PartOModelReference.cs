// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// Which model a Part O result was derived from: its <b>identity</b>, and where it was when the result was made.
    ///
    /// <para><b>Identity first, locator second, name last</b></para>
    /// <list type="bullet">
    /// <item><see cref="Guid"/> - the model's own guid: the same model by family. Two Save-As copies share it.</item>
    /// <item><see cref="Fingerprint"/> - the model's state: <c>SimulationResultProvenance.Fingerprint</c> of a design,
    /// or the <c>Fingerprint_Model</c> of a result's own provenance. It says whether the model is still the one the
    /// result was derived from, which the guid cannot.</item>
    /// <item><see cref="Path_Relative"/> - a <b>locator</b> only: where to look, from the folder the result model is written to, so a whole
    /// case tree that is moved or copied still finds its design. A file found there is accepted only when its identity
    /// matches. <b>No absolute path is ever persisted</b>: a saved result can be shared as a fixture or as evidence, and a
    /// workstation, user or OneDrive path in it would travel with it. Where a caller knows a place to look now, it hands it to
    /// <c>Query.PartOModelResolution</c> for that call only.</item>
    /// <item><see cref="Name"/> - for display. It is never used to find or accept anything.</item>
    /// </list>
    /// </summary>
    public class PartOModelReference : IJSAMObject, IAnalyticalObject
    {
        public PartOModelReference()
        {
        }

        public PartOModelReference(PartOModelReferenceKind kind, Guid guid, string name, string fingerprint, string path_Relative)
        {
            Kind = kind;
            Guid = guid;
            Name = name;
            Fingerprint = fingerprint ?? string.Empty;
            Path_Relative = path_Relative;
        }

        public PartOModelReference(PartOModelReference partOModelReference)
        {
            if (partOModelReference is not null)
            {
                Kind = partOModelReference.Kind;
                Guid = partOModelReference.Guid;
                Name = partOModelReference.Name;
                Fingerprint = partOModelReference.Fingerprint;
                Path_Relative = partOModelReference.Path_Relative;
            }
        }

        public PartOModelReference(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>A design model or a result model.</summary>
        public PartOModelReferenceKind Kind { get; set; } = PartOModelReferenceKind.Undefined;

        /// <summary>The referenced model's guid. Required.</summary>
        public Guid Guid { get; set; } = Guid.Empty;

        /// <summary>The referenced model's name when recorded. Display only.</summary>
        public string Name { get; set; }

        /// <summary>The referenced model's state fingerprint when recorded. Required.</summary>
        public string Fingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Where the model was when the result was made, relative to the folder the result model is written to. A locator, may be null; never
        /// absolute, and the only path that is persisted.
        /// </summary>
        public string Path_Relative { get; set; }

        /// <summary>A kind, a guid and a fingerprint: enough to recognise the model. A locator is optional.</summary>
        public bool IsValid => Kind != PartOModelReferenceKind.Undefined && Guid != Guid.Empty && !string.IsNullOrEmpty(Fingerprint);

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject is null)
            {
                return false;
            }

            Kind = Enum.TryParse(Text(jsonObject, "Kind"), out PartOModelReferenceKind kind) ? kind : PartOModelReferenceKind.Undefined;
            Guid = Guid.TryParse(Text(jsonObject, "Guid"), out Guid guid) ? guid : Guid.Empty;
            Name = Text(jsonObject, "Name");
            Fingerprint = Text(jsonObject, "Fingerprint") ?? string.Empty;
            Path_Relative = Text(jsonObject, "Path_Relative");

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject result = new()
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["Kind"] = Kind.ToString(),
                ["Guid"] = Guid.ToString(),
                ["Fingerprint"] = Fingerprint ?? string.Empty,
            };

            if (Name is not null)
            {
                result["Name"] = Name;
            }

            if (Path_Relative is not null)
            {
                result["Path_Relative"] = Path_Relative;
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} '{1}' ({2})", Kind, Name, Guid);
        }

        internal static string Text(JsonObject jsonObject, string name)
        {
            return jsonObject[name] is JsonValue jsonValue && jsonValue.TryGetValue(out string result) ? result : null;
        }
    }
}
