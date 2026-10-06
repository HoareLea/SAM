// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One Approved Document O overheating assessment, stated as engineering intent: <b>which</b> part of
    /// the building is being assessed, <b>at what mitigation stage</b>, <b>with which system</b>, and
    /// <b>under what operating assumptions</b>.
    /// <para>
    /// <b>Intent, not execution.</b> A scenario says what is to be assessed and says nothing whatever about
    /// how. There is no simulation engine here, no file path, no TSD or TPD, no plant-room object and no
    /// result. That boundary is what lets the same scenario be answered by a simple TSD run today and a
    /// full HVAC TPD run later without its identity moving - and it is why the routing decision belongs to
    /// the TAS adapter, which is in a different assembly, on the other side of a reference this one does
    /// not have.
    /// </para>
    /// <para>
    /// <b><see cref="Key"/> is derived, never generated and never stored.</b> Two engineers on two machines
    /// who state the same assessment get the same guid, so results can be attributed to a scenario that was
    /// never saved anywhere, and a scenario reloaded from JSON is recognisably the same one. A
    /// <c>Guid.NewGuid()</c> would have made every rebuild of the same intent a new assessment, which is
    /// exactly the failure the Part F annotation keys were rewritten to avoid.
    /// </para>
    ///
    /// <para><b>What the key is made of, and why each part is in it</b></para>
    /// <list type="bullet">
    /// <item><b>Scope and design zone guid</b> - the thing being assessed, by stable design identity.
    /// <b>Never a name</b>: every flat in a block has a "Bedroom 2" and half of them are called "Flat 2" in
    /// somebody's model. Renaming a flat must not orphan its assessment, and two flats that happen to share
    /// a name must not share one.</item>
    /// <item><b>Iteration</b> - the mitigation stage. The same dwelling at base provision and with acoustic
    /// restriction are two engineering answers.</item>
    /// <item><b>System template</b> - the existing <see cref="SystemTemplate"/> identity, field by field.
    /// <b>No second equipment vocabulary is introduced here.</b> <c>MVRE</c> already is SAM's heat-recovery
    /// ventilation and its template is the one with the exchanger in it; adding an <c>MVHR</c> alongside
    /// would split an established concept in two. Boost and summer bypass are operating states of a system,
    /// so they belong in the assumptions, not in a new system type.</item>
    /// <item><b>Operating assumptions</b> - what makes two runs of the same dwelling on the same system two
    /// different assessments.</item>
    /// </list>
    ///
    /// <para><b>What is deliberately NOT in the key</b></para>
    /// <list type="bullet">
    /// <item><see cref="Name"/> - presentation. A scenario renamed is the same assessment.</item>
    /// <item><see cref="Source"/> - provenance only, exactly as on <c>TMOverheatingCalculator</c>. Where an
    /// answer came from is not part of what the question was.</item>
    /// <item>Anything engine-shaped - TAS, TSD, TPD, file paths, weather files, run settings. None of it is
    /// a member of this type at all, so it cannot reach the key even by accident.</item>
    /// <item>Simulation output, view settings, and the time the scenario was created. A key that moved when
    /// a result arrived would be useless for attributing that result.</item>
    /// </list>
    ///
    /// <para><b>The key cannot go stale</b></para>
    /// <para>
    /// It is never persisted and never round-tripped through JSON, so there is no stored copy to disagree
    /// with anything. The identity-defining state has no public setter, and the two mutable objects it is
    /// built from - <see cref="SystemTemplate"/> and <see cref="OverheatingOperatingAssumptions"/> - are
    /// copied on the way in and copied on the way out, so neither the caller's instance nor one read back
    /// off the property is the one the key is derived from. Exactly two paths write that state: the
    /// constructors, which derive nothing until asked, and <see cref="FromJsonObject(JsonObject)"/>, which
    /// discards the derived key before it writes. There is no sequence of calls that leaves a scenario
    /// reporting a key that does not describe it.
    /// </para>
    ///
    /// <para><b>The encoding is canonical, not convenient</b></para>
    /// <para>
    /// Every component is UTF-8 and length-prefixed, in one fixed order, behind the schema marker
    /// <c>OverheatingScenario:v1</c>. Length prefixes rather than separators because concatenation is
    /// ambiguous - an assumption called <c>AB</c> with value <c>C</c> and one called <c>A</c> with value
    /// <c>BC</c> must not derive one key. Text is normalised to NFC, so an accent written as one code point
    /// and as a combining pair are one name. Enums are hashed by <b>name</b>, so inserting a member into
    /// <see cref="PartOIteration"/> later cannot silently renumber existing assessments. Numbers go through
    /// <c>OverheatingOperatingAssumptions.Text(double)</c>, which is invariant of both the machine's locale
    /// and its .NET runtime. And UTF-8 rather than <c>Core.Query.ComputeHash</c>, which encodes ASCII: it
    /// maps every non-ASCII character to <c>?</c>, so under it <c>café</c> and <c>cafè</c> are the same
    /// string. Fine for a checksum, unusable for an identity.
    /// </para>
    /// </summary>
    public class OverheatingScenario : IJSAMObject, IAnalyticalObject
    {
        /// <summary>
        /// The identity schema marker, hashed first. <b>Bump it only for a deliberate, breaking change to
        /// what the key is made of</b> - doing so re-keys every scenario in existence, which is the point:
        /// a key derived under different rules must not be mistaken for one derived under these.
        /// <para>
        /// <c>static readonly</c> rather than <c>const</c>, which the compiler would inline into every
        /// consuming assembly - so a bump here would leave any assembly that was not rebuilt deriving keys
        /// under the old marker while believing it was current.
        /// </para>
        /// </summary>
        public static readonly string IdentitySchema = "OverheatingScenario:v1";

        /// <summary>
        /// Namespace for the derivation, so a scenario key can only ever collide with another scenario key
        /// and never with a real model guid or a Part F annotation key.
        /// </summary>
        private static readonly Guid guid_Namespace = new("2b7c9d14-5e63-4a8f-9c02-7d5a1e46b3f8");

        private PartOAssessmentScope partOAssessmentScope = PartOAssessmentScope.Undefined;
        private Guid guid_Zone = Guid.Empty;
        private PartOIteration partOIteration = PartOIteration.Undefined;
        private SystemTemplate systemTemplate = null;
        private OverheatingOperatingAssumptions overheatingOperatingAssumptions = new();

        /// <summary>
        /// The derived key, held after its first read. <b>Not a cache over mutable state.</b> The only
        /// paths that write identity-defining state are the constructors - which leave this unset, so the
        /// first read derives it - and <see cref="FromJsonObject(JsonObject)"/>, which clears it before it
        /// writes anything. There is therefore no state this can outlive.
        /// <para>
        /// It exists because <see cref="GetHashCode"/> is the key: without it, putting scenarios in a
        /// <c>HashSet</c> - which is how results will be attributed to them - costs a SHA-256 per lookup.
        /// </para>
        /// </summary>
        private Guid? guid_Key = null;

        public OverheatingScenario()
        {

        }

        /// <param name="partOAssessmentScope">Whether this assesses a dwelling or a common space.</param>
        /// <param name="guid_Zone">
        /// The <b>design</b> zone's guid - the identity that survives a rename and a simulation round trip.
        /// </param>
        /// <param name="partOIteration">The mitigation stage stated.</param>
        /// <param name="systemTemplate">
        /// The system identity, copied. An invalid or entirely blank template is stored as none, because
        /// "no system stated" and "a system with nothing stated about it" are the same statement.
        /// </param>
        /// <param name="overheatingOperatingAssumptions">The operating assumptions, copied.</param>
        public OverheatingScenario(PartOAssessmentScope partOAssessmentScope, Guid guid_Zone, PartOIteration partOIteration, SystemTemplate systemTemplate = null, OverheatingOperatingAssumptions overheatingOperatingAssumptions = null)
        {
            this.partOAssessmentScope = partOAssessmentScope;
            this.guid_Zone = guid_Zone;
            this.partOIteration = partOIteration;

            //Copied, not referenced: SystemTemplate is mutable, and a caller who kept their instance could
            //otherwise change this scenario's identity after the fact.
            this.systemTemplate = Normalized(systemTemplate);
            this.overheatingOperatingAssumptions = new OverheatingOperatingAssumptions(overheatingOperatingAssumptions);
        }

        public OverheatingScenario(OverheatingScenario overheatingScenario)
        {
            if (overheatingScenario != null)
            {
                partOAssessmentScope = overheatingScenario.partOAssessmentScope;
                guid_Zone = overheatingScenario.guid_Zone;
                partOIteration = overheatingScenario.partOIteration;
                systemTemplate = Normalized(overheatingScenario.systemTemplate);
                overheatingOperatingAssumptions = new OverheatingOperatingAssumptions(overheatingScenario.overheatingOperatingAssumptions);

                Name = overheatingScenario.Name;
                Source = overheatingScenario.Source;
            }
        }

        public OverheatingScenario(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>
        /// A label for people. <b>Presentation only</b> - it takes no part in <see cref="Key"/>, so a
        /// scenario renamed is still the same assessment and two scenarios sharing a name are still two.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Where this scenario came from - a model name, a workflow, a user. <b>Provenance only</b>, on the
        /// same terms as <c>TMOverheatingCalculator.Source</c>: it names no object, owns no result and
        /// takes no part in any identity.
        /// </summary>
        public string Source { get; set; }

        /// <summary>Whether a dwelling or a common space is being assessed. Identity-defining.</summary>
        public PartOAssessmentScope Scope => partOAssessmentScope;

        /// <summary>
        /// The design zone being assessed, by guid. Identity-defining, and <b>the reason a name is not</b>.
        /// </summary>
        public Guid ZoneGuid => guid_Zone;

        /// <summary>The mitigation stage stated. Identity-defining.</summary>
        public PartOIteration Iteration => partOIteration;

        /// <summary>
        /// The system identity, or null where none is stated. Identity-defining. <b>A copy</b> - changing
        /// what this returns changes nothing.
        /// </summary>
        public SystemTemplate SystemTemplate => systemTemplate == null ? null : new SystemTemplate(systemTemplate);

        /// <summary>
        /// The operating assumptions. Identity-defining. <b>A copy</b> - changing what this returns changes
        /// nothing.
        /// </summary>
        public OverheatingOperatingAssumptions OperatingAssumptions => new(overheatingOperatingAssumptions);

        /// <summary>
        /// The ventilation strategy the scenario states - <c>NV</c>, <c>MV</c>, <c>MVRE</c>, <c>UV</c> -
        /// or null where no system is stated.
        /// <para>
        /// <b>Not a second vocabulary.</b> This names the existing <see cref="SystemTemplate"/> field and
        /// introduces no identity of its own; in particular there is no <c>MVHR</c>, because <c>MVRE</c>
        /// already is SAM's heat-recovery ventilation and splitting an established concept in two would
        /// make one system two.
        /// </para>
        /// <para>
        /// <b>This is now the authoritative statement of the strategy</b>, carried to the assessment by
        /// <c>VentilationStrategyMap</c> and read by <c>TMOverheatingCalculator</c> in place of the three
        /// derivations that used to disagree - the space's internal condition, any related mechanical
        /// <c>VentilationSystem</c>, and a zone-<i>name</i> lookup against a system library that defaulted to
        /// <c>"NV"</c> and so assessed an MVRE dwelling against the natural-ventilation criterion.
        /// </para>
        /// <para>
        /// A consumer reading this must <b>refuse</b> where <see cref="HasVentilationStrategy"/> is false
        /// rather than fall back to that chain; a silent default is the defect, not the absence of one.
        /// </para>
        /// </summary>
        public string VentilationStrategy => systemTemplate?.Ventilation;

        /// <summary>
        /// Whether the scenario actually states a ventilation strategy. A scenario can be
        /// <see cref="IsValid"/> without one - naming a dwelling is a complete statement of what is being
        /// assessed - so this is the separate question a consumer of
        /// <see cref="VentilationStrategy"/> has to ask.
        /// </summary>
        public bool HasVentilationStrategy => !string.IsNullOrWhiteSpace(VentilationStrategy);

        /// <summary>
        /// The scenario's identity, derived from its engineering content.
        /// <para>
        /// <b><see cref="Guid.Empty"/> where the scenario is not valid</b>, because a scenario that names
        /// nothing assessable has no identity to have. Deriving one anyway would give every half-filled
        /// scenario in a user interface the same real-looking key and let them collide silently in a set;
        /// empty says "nothing", which is the truth, and reads as such everywhere else in SAM.
        /// </para>
        /// </summary>
        public Guid Key => IsValid ? guid_Key ?? (Guid)(guid_Key = Derive()) : Guid.Empty;

        /// <summary>
        /// Whether the scenario names something assessable: a stated scope over a real design zone. The
        /// iteration and the system may legitimately be unstated - a foundation-stage scenario over a real
        /// dwelling is a complete statement.
        /// </summary>
        public bool IsValid => partOAssessmentScope != PartOAssessmentScope.Undefined && guid_Zone != Guid.Empty;

        /// <summary>
        /// Two scenarios are the same assessment when they derive the same key.
        /// <para>
        /// <b>A scenario that names nothing assessable is equal only to itself.</b> It has no identity - its
        /// <see cref="Key"/> is empty - and treating "no identity" as an identity they all share would
        /// collapse every half-filled scenario a user interface is holding into one entry of a
        /// <c>HashSet</c>. Reference equality is the honest answer for something that has not said what it
        /// is yet.
        /// </para>
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is OverheatingScenario overheatingScenario))
            {
                return false;
            }

            return IsValid && overheatingScenario.IsValid && Key == overheatingScenario.Key;
        }

        /// <summary>
        /// The key where there is one, and the identity hash where there is not - so this agrees with
        /// <see cref="Equals(object)"/> in both cases, which is what a <c>HashSet</c> lookup depends on.
        /// </summary>
        public override int GetHashCode()
        {
            return IsValid ? Key.GetHashCode() : base.GetHashCode();
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
            {
                return false;
            }

            //This is the one path that writes identity-defining state after construction, so it is also the
            //one place the held key has to be dropped. Cleared first, before anything below can throw.
            guid_Key = null;

            //Nothing is required, but an object with neither of these is not a scenario at all - and
            //reporting success on it would hand the caller an empty assessment that looks loaded. Read
            //before any state is written, so a rejected object leaves this one alone.
            string text_Scope = Text(jsonObject, "Scope");
            string text_ZoneGuid = Text(jsonObject, "ZoneGuid");

            if (text_Scope == null && text_ZoneGuid == null)
            {
                return false;
            }

            Name = Text(jsonObject, "Name");
            Source = Text(jsonObject, "Source");

            //Enums are read by name, and a name this build does not know is Undefined rather than an
            //exception: a scenario written by a later version must not make a file unreadable, and
            //Undefined is visibly not an assessment rather than quietly the first member. Enum.TryParse
            //also accepts numeric text - "99" would parse to (PartOIteration)99, a mitigation stage that
            //does not exist - so what it returns is checked against the members that do.
            partOAssessmentScope = Enum.TryParse(text_Scope ?? string.Empty, out PartOAssessmentScope partOAssessmentScope_Temp) && Enum.IsDefined(typeof(PartOAssessmentScope), partOAssessmentScope_Temp) ? partOAssessmentScope_Temp : PartOAssessmentScope.Undefined;

            string text_Iteration = Text(jsonObject, "Iteration");

            partOIteration = Enum.TryParse(text_Iteration ?? string.Empty, out PartOIteration partOIteration_Temp) && Enum.IsDefined(typeof(PartOIteration), partOIteration_Temp) ? partOIteration_Temp : PartOIteration.Undefined;

            guid_Zone = Guid.TryParse(text_ZoneGuid ?? string.Empty, out Guid guid_Zone_Temp) ? guid_Zone_Temp : Guid.Empty;

            systemTemplate = jsonObject["SystemTemplate"] is JsonObject jsonObject_SystemTemplate ? Normalized(new SystemTemplate(jsonObject_SystemTemplate)) : null;

            overheatingOperatingAssumptions = jsonObject["OperatingAssumptions"] is JsonObject jsonObject_OperatingAssumptions ? new OverheatingOperatingAssumptions(jsonObject_OperatingAssumptions) : new OverheatingOperatingAssumptions();

            return true;
        }

        public JsonObject ToJsonObject()
        {
            //Key is deliberately absent. Writing it would create a second, storable copy of an identity that
            //is supposed to have exactly one source - the state below - and a file edited by hand or written
            //by an older version could then assert a key that does not describe its own contents.
            JsonObject jsonObject = new()
            {
                ["_type"] = Core.Query.FullTypeName(this)
            };

            if (Name != null)
            {
                jsonObject["Name"] = Name;
            }

            if (Source != null)
            {
                jsonObject["Source"] = Source;
            }

            //By name, matching the derivation: a renumbered enum must not re-key stored scenarios.
            jsonObject["Scope"] = partOAssessmentScope.ToString();
            jsonObject["ZoneGuid"] = guid_Zone.ToString("D", CultureInfo.InvariantCulture);
            jsonObject["Iteration"] = partOIteration.ToString();

            if (systemTemplate != null)
            {
                jsonObject["SystemTemplate"] = systemTemplate.ToJsonObject();
            }

            jsonObject["OperatingAssumptions"] = overheatingOperatingAssumptions.ToJsonObject();

            return jsonObject;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} [{3}]", partOAssessmentScope, partOIteration, systemTemplate == null ? "-" : systemTemplate.ToString(), guid_Zone.ToString("D", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The copy of a system identity a scenario holds: rebuilt through
        /// <see cref="SystemTemplate"/>'s own property setters, and null where nothing is stated.
        /// <para>
        /// <b>Rebuilt rather than copy-constructed, and that is not tidiness.</b> Those setters strip
        /// spaces, and <c>SystemTemplate</c>'s copy and JSON constructors both assign its fields raw - so
        /// <c>new SystemTemplate("MV RE", …)</c> stores <c>MVRE</c> while the same template arriving from
        /// JSON as <c>{"Ventilation": "MV RE"}</c> stores <c>MV RE</c>. <c>SystemTemplate</c> treats those
        /// as one identity; promoting its fields into a key would otherwise make them two. Normalising at
        /// this boundary fixes it for scenario identity without changing a serialisation path the rest of
        /// SAM shares.
        /// </para>
        /// </summary>
        private static SystemTemplate Normalized(SystemTemplate systemTemplate)
        {
            if (systemTemplate == null || !systemTemplate.IsValid)
            {
                return null;
            }

            return new SystemTemplate(systemTemplate.Ventilation, systemTemplate.Heating, systemTemplate.Cooling, systemTemplate.PlantRoom, systemTemplate.Controls, systemTemplate.Version);
        }

        /// <summary>
        /// The text of a JSON property, or null where it is absent or is not text.
        /// <para>
        /// <c>GetValue&lt;string&gt;()</c> throws on a JSON number or boolean, which would make one
        /// hand-edited property cost the whole model rather than that property.
        /// </para>
        /// </summary>
        private static string Text(JsonObject jsonObject, string name)
        {
            if (jsonObject == null || name == null || !jsonObject.ContainsKey(name))
            {
                return null;
            }

            return jsonObject[name] is JsonValue jsonValue && jsonValue.TryGetValue(out string result) ? result : null;
        }

        /// <summary>
        /// Derives the key: the namespace, then every identity-defining component in one fixed order, each
        /// UTF-8 and length-prefixed; SHA-256; the first sixteen bytes stamped with the version and variant
        /// bits, in the manner of RFC 4122's name-based UUIDs and exactly as <c>PartFAnnotationKey</c> does.
        /// <para>
        /// The hash is a spreading function and not a security primitive - the whole point is that it is
        /// reproducible, which is why it is not salted and must never become so.
        /// </para>
        /// </summary>
        private Guid Derive()
        {
            List<byte> bytes = [.. guid_Namespace.ToByteArray()];

            //First, and before anything that could change: a key derived under a different schema must be a
            //different key.
            Append(bytes, IdentitySchema);

            //The thing being assessed. Guid formatted invariantly as text so every component of the
            //derivation goes through one encoding rule and none through a byte layout.
            Append(bytes, partOAssessmentScope.ToString());
            Append(bytes, guid_Zone.ToString("D", CultureInfo.InvariantCulture));

            //The mitigation stage.
            Append(bytes, partOIteration.ToString());

            //The system, field by field rather than through ToString(): ToString() is a display format that
            //may reasonably be improved one day, and improving it must not re-key every assessment.
            Append(bytes, systemTemplate?.Ventilation);
            Append(bytes, systemTemplate?.Heating);
            Append(bytes, systemTemplate?.Cooling);
            Append(bytes, systemTemplate?.PlantRoom);
            Append(bytes, systemTemplate?.Controls);
            Append(bytes, systemTemplate?.Version);

            //The assumptions, in their canonical ordinal order, counted first so a trailing assumption
            //cannot be confused with a longer value on the one before it.
            List<KeyValuePair<string, string>> keyValuePairs = overheatingOperatingAssumptions.ToList();

            AppendLength(bytes, keyValuePairs.Count);

            foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
            {
                Append(bytes, keyValuePair.Key);
                Append(bytes, keyValuePair.Value);
            }

            byte[] hash;

            using (System.Security.Cryptography.SHA256 sHA256 = System.Security.Cryptography.SHA256.Create())
            {
                hash = sHA256.ComputeHash([.. bytes]);
            }

            byte[] result = new byte[16];
            Array.Copy(hash, result, 16);

            //Version 8 (custom) and the RFC 4122 variant, so this is a valid guid and is visibly not a model
            //identity that happens to look similar.
            result[7] = (byte)((result[7] & 0x0F) | 0x80);
            result[8] = (byte)((result[8] & 0x3F) | 0x80);

            return new Guid(result);
        }

        /// <summary>
        /// Appends one component: its UTF-8 length, then its UTF-8 bytes. Length-prefixed rather than
        /// separated, so no combination of component values can produce the byte sequence of a different
        /// combination. A null component is written as length -1, which is distinct from an empty one:
        /// "not stated" and "stated as blank" are different statements.
        /// <para>
        /// Normalised to NFC through <c>OverheatingOperatingAssumptions.Canonical</c>, the one
        /// implementation - the same accented character has two encodings, and text typed on macOS is
        /// routinely the second. This catches the components that do not go through the assumptions bag,
        /// which normalises its names on the way in because it also <i>sorts</i> by them.
        /// </para>
        /// </summary>
        private static void Append(List<byte> bytes, string text)
        {
            if (text == null)
            {
                AppendLength(bytes, -1);
                return;
            }

            byte[] bytes_Text = Encoding.UTF8.GetBytes(OverheatingOperatingAssumptions.Canonical(text));

            AppendLength(bytes, bytes_Text.Length);
            bytes.AddRange(bytes_Text);
        }

        /// <summary>
        /// Writes an int as four bytes, least significant first, explicitly rather than through
        /// <c>BitConverter</c> - which is endian-dependent, and a key must not depend on the architecture
        /// that derived it.
        /// </summary>
        private static void AppendLength(List<byte> bytes, int value)
        {
            unchecked
            {
                bytes.Add((byte)value);
                bytes.Add((byte)(value >> 8));
                bytes.Add((byte)(value >> 16));
                bytes.Add((byte)(value >> 24));
            }
        }
    }
}
