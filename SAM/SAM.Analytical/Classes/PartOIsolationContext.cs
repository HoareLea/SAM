// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// States that a model is the <b>isolated</b> derived model of a Part O run rather than the whole
    /// building, and says exactly which dwellings it was derived for.
    ///
    /// <para><b>Why the model cannot state this by itself</b></para>
    /// <para>
    /// An isolated model is a real, self-consistent <see cref="AnalyticalModel"/> containing only the
    /// selected dwellings' spaces. Reopened a year later it is indistinguishable from a building that only
    /// ever had those spaces in it - the geometry carries no record of what was cut away. So a reviewer
    /// reading <c>Flat 1</c> results could not tell whether they came from a whole-building simulation or
    /// from an isolated one, and the two are <b>different thermal models</b> (see
    /// <see cref="Adiabatic"/> below). This record is what makes the difference reviewable, and it is
    /// stamped on the model so it travels into the run's <c>.sam</c> with everything else.
    /// </para>
    ///
    /// <para><b>Never reconstructed from a filename</b></para>
    /// <para>
    /// <see cref="ScopeToken"/> also appears in the run's project name so two isolated runs of one building
    /// do not overwrite each other's evidence - but that is <i>naming only</i>. Nothing reads isolation
    /// state back out of a path: a file can be renamed, and a renamed file must not be able to change what a
    /// run is understood to have been. This object is the authority.
    /// </para>
    ///
    /// <para><b>The engineering assumption this record exists to disclose</b></para>
    /// <para id="Adiabatic">
    /// Interfaces between a selected space and an excluded one are simulated as <b>adiabatic</b>: the
    /// omitted neighbouring conditioned space is assumed to pass approximately zero net conductive heat
    /// across the cut. That is the standard isolated-zone assumption and it is why isolation is fast, but it
    /// means an isolated result is <b>not</b> the whole-building result for the same flat and must never be
    /// presented as one.
    /// </para>
    /// </summary>
    public class PartOIsolationContext : SAMObject
    {
        public PartOIsolationContext()
        {
        }

        /// <param name="guids_Space">The isolated model's thermal spaces - the selection, by identity.</param>
        /// <param name="guids_Zone">The dwelling zones those spaces were selected as.</param>
        /// <param name="names_Dwelling">What to call those dwellings in a report. Display only.</param>
        public PartOIsolationContext(IEnumerable<Guid> guids_Space, IEnumerable<Guid> guids_Zone, IEnumerable<string> names_Dwelling)
        {
            foreach (Guid guid in guids_Space ?? [])
            {
                Guids_Space.Add(guid);
            }

            foreach (Guid guid in guids_Zone ?? [])
            {
                Guids_Zone.Add(guid);
            }

            foreach (string name in names_Dwelling ?? [])
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    Names_Dwelling.Add(name);
                }
            }

            ScopeToken = Token(Guids_Space);
        }

        public PartOIsolationContext(PartOIsolationContext partOIsolationContext)
            : base(partOIsolationContext)
        {
            if (partOIsolationContext is not null)
            {
                Guids_Space.AddRange(partOIsolationContext.Guids_Space);
                Guids_Zone.AddRange(partOIsolationContext.Guids_Zone);
                Names_Dwelling.AddRange(partOIsolationContext.Names_Dwelling);
                ScopeToken = partOIsolationContext.ScopeToken;
            }
        }

        public PartOIsolationContext(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>The spaces the isolated model actually simulates, by identity. Never by name.</summary>
        public List<Guid> Guids_Space { get; } = [];

        /// <summary>The dwelling zones the user selected, by identity.</summary>
        public List<Guid> Guids_Zone { get; } = [];

        /// <summary>
        /// The dwelling names, for a report to read. <b>Display only</b> - two dwellings may share a name,
        /// which is exactly why the guids above are the authority.
        /// </summary>
        public List<string> Names_Dwelling { get; } = [];

        /// <summary>
        /// A short, stable identifier for <i>this</i> selection, derived from the space guids alone - so the
        /// same selection always produces the same token and any other selection produces a different one.
        /// Used to keep one isolated run's artifacts from overwriting another's. <b>Naming only.</b>
        /// </summary>
        public string ScopeToken { get; private set; } = string.Empty;

        /// <summary>Whether this record actually states a scope. An empty one states nothing and is not a record.</summary>
        public bool IsValid => Guids_Space.Count != 0 && !string.IsNullOrWhiteSpace(ScopeToken);

        /// <summary>
        /// The scope token for a set of spaces: an FNV-1a digest over the guids, <b>sorted</b>, so it is a
        /// function of the selection and not of the order it happened to be enumerated in. Eight hex
        /// characters - short enough to sit in a filename, wide enough that two selections on one project
        /// will not collide in practice.
        /// </summary>
        public static string Token(IEnumerable<Guid> guids_Space)
        {
            List<string> texts = [];
            foreach (Guid guid in guids_Space ?? [])
            {
                texts.Add(guid.ToString("N"));
            }

            if (texts.Count == 0)
            {
                return string.Empty;
            }

            texts.Sort(StringComparer.Ordinal);

            //FNV-1a, 64 bit. Not a security hash - an identity digest, chosen to match the digests used
            //elsewhere in the Part O evidence chain rather than to introduce a second convention.
            ulong hash = 14695981039346656037;
            foreach (byte @byte in Encoding.UTF8.GetBytes(string.Join("|", texts)))
            {
                hash ^= @byte;
                hash *= 1099511628211;
            }

            return hash.ToString("x16").Substring(0, 8);
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            Guids_Space.Clear();
            Guids_Zone.Clear();
            Names_Dwelling.Clear();

            foreach (string text in Texts(jsonObject, "Guids_Space"))
            {
                if (Guid.TryParse(text, out Guid guid))
                {
                    Guids_Space.Add(guid);
                }
            }

            foreach (string text in Texts(jsonObject, "Guids_Zone"))
            {
                if (Guid.TryParse(text, out Guid guid))
                {
                    Guids_Zone.Add(guid);
                }
            }

            Names_Dwelling.AddRange(Texts(jsonObject, "Names_Dwelling"));

            ScopeToken = jsonObject["ScopeToken"]?.GetValue<string>() ?? string.Empty;

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject jsonObject = base.ToJsonObject();
            if (jsonObject is null)
            {
                return null;
            }

            jsonObject["Guids_Space"] = Array(Guids_Space.ConvertAll(x => x.ToString()));
            jsonObject["Guids_Zone"] = Array(Guids_Zone.ConvertAll(x => x.ToString()));
            jsonObject["Names_Dwelling"] = Array(Names_Dwelling);
            jsonObject["ScopeToken"] = ScopeToken;

            return jsonObject;
        }

        private static JsonArray Array(IEnumerable<string> texts)
        {
            JsonArray jsonArray = [];
            foreach (string text in texts ?? [])
            {
                jsonArray.Add(text);
            }

            return jsonArray;
        }

        private static List<string> Texts(JsonObject jsonObject, string name)
        {
            List<string> result = [];

            if (jsonObject?[name] is not JsonArray jsonArray)
            {
                return result;
            }

            foreach (JsonNode jsonNode in jsonArray)
            {
                string text = jsonNode?.GetValue<string>();
                if (text is not null)
                {
                    result.Add(text);
                }
            }

            return result;
        }
    }
}
