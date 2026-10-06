// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The named operating assumptions that make one overheating assessment a different assessment from
    /// another over the same fabric - how the openings are operated, whether a mechanical system's boost or
    /// summer bypass is available, and so on.
    /// <para>
    /// <b>Identity-defining, and that is the point.</b> Two runs of the same dwelling with the same system
    /// but different opening behaviour are two engineering answers, not one answer computed twice, and they
    /// must not share a key. Everything in here therefore participates in
    /// <c>OverheatingScenario.Key</c>.
    /// </para>
    /// <para>
    /// <b>A bag of names and values rather than a typed set, deliberately.</b> The assumptions that define
    /// the mitigated Part O iterations are not settled yet, and inventing an enum for each of them now
    /// would fix a vocabulary before the engineering that uses it exists. What this does have to be is
    /// canonical: the assumptions are ordered by name, ordinally, so a scenario assembled in a different
    /// order derives the same key, and values are formatted invariantly so a machine's locale can never
    /// change an identity. The typed <see cref="Set(string, double)"/> and <see cref="Set(string, bool)"/>
    /// overloads exist so a caller cannot reach for <c>ToString()</c> and get a comma decimal separator
    /// into a key.
    /// </para>
    /// <para>
    /// <b>Not a parameter set.</b> <c>ParameterSet</c> already exists and is the right thing for values
    /// carried on a model object. This is a small immutable-in-practice value whose entire job is to hash
    /// canonically, and reusing <c>ParameterSet</c> would have meant depending on its ordering and its JSON
    /// shape as an identity contract - which they are not.
    /// </para>
    /// </summary>
    public class OverheatingOperatingAssumptions : IJSAMObject, IAnalyticalObject
    {
        /// <summary>
        /// Nine decimal places, rounded identically by every runtime this assembly loads under. See
        /// <see cref="Text(double)"/>.
        /// </summary>
        private const string format_Numeric = "0.#########";

        //Ordinal, not culture-aware: a key must not depend on the machine's collation, and "SS" must never
        //be equal to "ß".
        private readonly SortedDictionary<string, string> dictionary = new(StringComparer.Ordinal);

        public OverheatingOperatingAssumptions()
        {

        }

        public OverheatingOperatingAssumptions(OverheatingOperatingAssumptions overheatingOperatingAssumptions)
        {
            if (overheatingOperatingAssumptions != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in overheatingOperatingAssumptions.dictionary)
                {
                    dictionary[keyValuePair.Key] = keyValuePair.Value;
                }
            }
        }

        public OverheatingOperatingAssumptions(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>How many assumptions are stated.</summary>
        public int Count => dictionary.Count;

        /// <summary>
        /// The names stated, in the canonical ordinal order the key is derived in. A copy - adding to it
        /// changes nothing.
        /// </summary>
        public List<string> Names => [.. dictionary.Keys];

        /// <summary>Whether an assumption of this name is stated.</summary>
        public bool Contains(string name)
        {
            return name != null && dictionary.ContainsKey(Canonical(name));
        }

        /// <summary>The value stated for an assumption, or null where it is not stated.</summary>
        public string Value(string name)
        {
            return name != null && dictionary.TryGetValue(Canonical(name), out string result) ? result : null;
        }

        /// <summary>
        /// States an assumption. A blank name is ignored - an unnamed assumption cannot be read back and
        /// would only make the key depend on something invisible. A null value is stored as empty.
        /// <para>
        /// <b>The name is normalised here, before it is sorted, and that ordering is why.</b> The
        /// assumptions are hashed in ordinal name order, and ordinal order is taken over the raw code
        /// units: composed <c>é</c> is U+00E9 and sorts <i>after</i> <c>f</c>, while the decomposed form
        /// starts with <c>e</c> and sorts <i>before</i> it. Normalising only at the point of hashing would
        /// have left two canonically identical sets of assumptions hashed in different orders, and
        /// therefore two keys for one assessment - the exact failure normalising was added to prevent.
        /// </para>
        /// </summary>
        public void Set(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            dictionary[Canonical(name)] = Canonical(value) ?? string.Empty;
        }

        /// <summary>
        /// States a numeric assumption. <b>Use this rather than formatting the number yourself</b> - see
        /// <see cref="Text(double)"/> for the two ways a hand-formatted double splits one assessment in
        /// two.
        /// </summary>
        public void Set(string name, double value)
        {
            Set(name, Text(value));
        }

        /// <summary>States a boolean assumption, invariantly.</summary>
        public void Set(string name, bool value)
        {
            Set(name, Text(value));
        }

        /// <summary>
        /// The canonical text of a boolean assumption. Written out rather than taken from
        /// <c>bool.ToString()</c> so that the one form is what every route produces - the typed setter and
        /// a JSON <c>true</c>/<c>false</c> alike.
        /// </summary>
        public static string Text(bool value)
        {
            return value ? "True" : "False";
        }

        /// <summary>
        /// The canonical text of a numeric assumption - the form it is both stored and hashed in, so there
        /// is no hidden precision behind what a reader sees.
        /// <para>
        /// <b>Two things would otherwise split one assessment in two.</b> The obvious one is the machine's
        /// locale: a German-configured machine writes <c>21,5</c>. The less obvious one is the host
        /// runtime. <c>"R"</c> and <c>"G17"</c> both changed meaning in .NET Core 3.0 - .NET Framework
        /// formats through a 15-significant-digit intermediate and falls back to 17, .NET 5+ emits the
        /// shortest round-trippable form - so <c>2.0/3.0</c> is <c>0.66666666666666663</c> under one and
        /// <c>0.6666666666666666</c> under the other. This assembly is <c>netstandard2.0</c> precisely so
        /// it loads under both, and SAM has live .NET Framework consumers, so a round-trip format would mean
        /// the Revit-side process and the WPF-side process deriving two keys for one stated assessment.
        /// </para>
        /// <para>
        /// Fixed to <b>nine decimal places</b>, which both runtimes round to identically, and which is a
        /// true statement about the engineering: two ventilation rates differing at 10⁻¹⁰ l/s are not two
        /// operating assumptions. Negative zero is folded into zero for the same reason. The non-finite
        /// values are written out by name rather than through <c>ToString</c>, whose symbols are a
        /// culture's business.
        /// </para>
        /// </summary>
        public static string Text(double value)
        {
            if (double.IsNaN(value))
            {
                return "NaN";
            }

            if (double.IsPositiveInfinity(value))
            {
                return "Infinity";
            }

            if (double.IsNegativeInfinity(value))
            {
                return "-Infinity";
            }

            //-0.0 and 0.0 compare equal and are the same assumption; only the formatter tells them apart.
            if (value == 0)
            {
                value = 0;
            }

            return value.ToString(format_Numeric, CultureInfo.InvariantCulture);
        }

        /// <summary>Removes an assumption. Removing an unstated one does nothing.</summary>
        public bool Remove(string name)
        {
            return name != null && dictionary.Remove(Canonical(name));
        }

        /// <summary>
        /// Text in its canonical form: NFC, so an accent written as one code point and as a letter followed
        /// by a combining accent are one string. Text typed on macOS is routinely the second form.
        /// <para>
        /// Text that is not valid Unicode cannot be normalised and is returned as it stands. Hashing it is
        /// still deterministic, which is all an identity needs; refusing it would fail an assessment over a
        /// stray character in somebody's room name.
        /// </para>
        /// </summary>
        public static string Canonical(string text)
        {
            if (text == null)
            {
                return null;
            }

            try
            {
                return text.Normalize(NormalizationForm.FormC);
            }
            catch (ArgumentException)
            {
                return text;
            }
        }

        /// <summary>
        /// The assumptions as ordered name/value pairs, in the order the key is derived in. A copy.
        /// </summary>
        public List<KeyValuePair<string, string>> ToList()
        {
            return [.. dictionary];
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
            {
                return false;
            }

            dictionary.Clear();

            if (jsonObject["Assumptions"] is JsonObject jsonObject_Assumptions)
            {
                foreach (KeyValuePair<string, JsonNode> keyValuePair in jsonObject_Assumptions)
                {
                    if (TryGetText(keyValuePair.Value, out string value))
                    {
                        Set(keyValuePair.Key, value);
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

            JsonObject jsonObject_Assumptions = new();

            foreach (KeyValuePair<string, string> keyValuePair in dictionary)
            {
                jsonObject_Assumptions[keyValuePair.Key] = keyValuePair.Value;
            }

            jsonObject["Assumptions"] = jsonObject_Assumptions;

            return jsonObject;
        }

        /// <summary>
        /// The canonical text of a JSON assumption value, or false where the value is not something an
        /// assumption may be.
        /// <para>
        /// <b>A JSON primitive goes through exactly the same canonicaliser as the typed setter that would
        /// have written it.</b> <c>{"SummerBypass": false}</c> is what a person hand-editing a file would
        /// write, given that <see cref="Set(string, bool)"/> exists - and taking it at its literal JSON
        /// text would store <c>false</c> where the typed path stores <c>False</c>, so the same engineering
        /// assumption would derive two keys depending on which door it came through. Numbers have the same
        /// problem against <see cref="Text(double)"/>.
        /// </para>
        /// <para>
        /// <b>An object or an array is refused rather than flattened.</b> There is no canonical text for
        /// arbitrary JSON - property order alone would decide the key - and an operating assumption is a
        /// value, not a structure. Refusing drops that one assumption; it does not make the file
        /// unreadable.
        /// </para>
        /// </summary>
        private static bool TryGetText(JsonNode jsonNode, out string result)
        {
            result = null;

            if (jsonNode == null)
            {
                //A JSON null is an assumption stated as blank, which Set already distinguishes from one
                //that is not stated at all.
                return true;
            }

            if (!(jsonNode is JsonValue jsonValue))
            {
                return false;
            }

            //String first: a JSON string "21" is text that happens to look numeric and must stay as it is.
            if (jsonValue.TryGetValue(out string text))
            {
                result = text;
                return true;
            }

            if (jsonValue.TryGetValue(out bool boolean))
            {
                result = Text(boolean);
                return true;
            }

            if (jsonValue.TryGetValue(out double number))
            {
                result = Text(number);
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            List<string> strings = [];

            foreach (KeyValuePair<string, string> keyValuePair in dictionary)
            {
                strings.Add(string.Format(CultureInfo.InvariantCulture, "{0}={1}", keyValuePair.Key, keyValuePair.Value));
            }

            return string.Join(", ", strings);
        }
    }
}
