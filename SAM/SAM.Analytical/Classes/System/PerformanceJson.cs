// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The handful of tolerant readers the manufacturer performance classes share.
    /// <para>
    /// <b>Tolerant means "returns nothing", never "throws" and never "guesses".</b> A manufacturer
    /// catalogue is a hand-transcribed document. One mistyped cell in it must not take a model down, and
    /// it must not quietly become a plausible number either - so a value that is not what it claims to be
    /// reads as absent, which makes the object holding it invalid, which is a refusal the caller can see
    /// and name. That is the same posture <c>VentilationUnitReference</c> and
    /// <c>Systems.Query.SystemCapabilityDescriptors</c> already take towards hand-edited library files.
    /// </para>
    /// <para>
    /// Internal: these are an implementation detail of how the performance classes parse themselves, not
    /// a JSON utility the rest of SAM should reach for.
    /// </para>
    /// </summary>
    internal static class PerformanceJson
    {
        /// <summary>
        /// A string property, or null where it is absent or is not a JSON string.
        /// <para>
        /// <b>A JSON string only - never a stringified number, object, or array.</b> <c>"Source": 123</c> and
        /// <c>"Name": {}</c> are malformed data for this vocabulary, not a number or an object that happens
        /// to also be readable as text: <c>ToString()</c> on a non-string node returns the node's JSON text
        /// (<c>"123"</c>, <c>"{}"</c>), which would silently accept them as if they had been written
        /// correctly. Checked by <see cref="JsonNode.GetValueKind"/> rather than a type test, because every
        /// JSON scalar - string included - is represented by the same <see cref="JsonValue"/> node type.
        /// </para>
        /// </summary>
        internal static string Text(JsonObject jsonObject, string name)
        {
            if (jsonObject is null || !jsonObject.ContainsKey(name))
            {
                return null;
            }

            JsonNode jsonNode = jsonObject[name];

            return jsonNode is not null && jsonNode.GetValueKind() == System.Text.Json.JsonValueKind.String ? jsonNode.GetValue<string>() : null;
        }

        /// <summary>Writes a string property, omitting it entirely where there is nothing to say.</summary>
        internal static void SetText(JsonObject jsonObject, string name, string value)
        {
            if (jsonObject is not null && !string.IsNullOrWhiteSpace(value))
            {
                jsonObject[name] = value;
            }
        }

        /// <summary>
        /// An array of numbers, or null where the property is absent, is not an array, or holds anything
        /// that is not a finite number.
        /// <para>
        /// <b>All or nothing.</b> Skipping the bad element would silently shorten a performance axis or a
        /// value grid, and a grid one element short is a grid that no longer lines up with its axes - the
        /// one failure that would misread every subsequent number rather than losing one.
        /// </para>
        /// </summary>
        internal static double[] Values(JsonObject jsonObject, string name)
        {
            if (jsonObject is null || jsonObject[name] is not JsonArray jsonArray)
            {
                return null;
            }

            List<double> result = [];

            foreach (JsonNode jsonNode in jsonArray)
            {
                if (!TryGetDouble(jsonNode, out double value))
                {
                    return null;
                }

                result.Add(value);
            }

            return result.ToArray();
        }

        /// <summary>Writes an array of numbers, omitting it entirely where there is nothing to say.</summary>
        internal static void SetValues(JsonObject jsonObject, string name, double[] values)
        {
            if (jsonObject is null || values is null)
            {
                return;
            }

            JsonArray jsonArray = [];

            foreach (double value in values)
            {
                jsonArray.Add(value);
            }

            jsonObject[name] = jsonArray;
        }

        /// <summary>
        /// A single number, or <see cref="double.NaN"/> where the property is absent or is not a finite
        /// number.
        /// <para>
        /// <b>Absent and NaN are the same answer, deliberately</b> - a capacity nobody has established
        /// reads as NaN whether the catalogue omitted the key, wrote <c>null</c>, or wrote a note where a
        /// number should be. <c>VentilationUnitTemplate</c> depends on that: an unresolved capacity has to
        /// be one state, not three.
        /// </para>
        /// </summary>
        internal static double Value(JsonObject jsonObject, string name)
        {
            if (jsonObject is null || !jsonObject.ContainsKey(name))
            {
                return double.NaN;
            }

            return TryGetDouble(jsonObject[name], out double result) ? result : double.NaN;
        }

        /// <summary>Writes a number, omitting it entirely where it is not one - see <see cref="Value"/>.</summary>
        internal static void SetValue(JsonObject jsonObject, string name, double value)
        {
            if (jsonObject is not null && !double.IsNaN(value) && !double.IsInfinity(value))
            {
                jsonObject[name] = value;
            }
        }

        /// <summary>An integer property, or the fallback where it is absent or is not an integer.</summary>
        internal static int Integer(JsonObject jsonObject, string name, int value_Default)
        {
            return jsonObject is not null && jsonObject[name] is JsonValue jsonValue && jsonValue.TryGetValue(out int result) ? result : value_Default;
        }

        private static bool TryGetDouble(JsonNode jsonNode, out double value)
        {
            //Core.Query.TryGetDouble carries the reason this is not GetValue<object> + IsNumeric: a number
            //that has been PARSED is backed by a JsonElement, which is not a numeric CLR type. Rejecting
            //NaN and infinity stays here - it is this file's rule about what counts as a stated
            //performance figure, not a fact about reading JSON.
            return Core.Query.TryGetDouble(jsonNode, out value) && !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
