// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// Serialisation helpers shared by the Approved Document F data classes.
    /// <para>
    /// Kept internal and local to the Part F folder rather than added to the general Query and Modify
    /// surfaces: these are round-tripping details of one feature's own classes, not a vocabulary the rest
    /// of SAM should be reading a model with.
    /// </para>
    /// <para>
    /// The rule throughout is that an absent key and a null value both mean "not established", and NaN
    /// and infinity are never written to file. A missing rate must stay missing after a round trip,
    /// because "no value" and "zero l/s" are different engineering answers.
    /// </para>
    /// </summary>
    internal static class PartFJson
    {
        /// <summary>
        /// Reads an optional number. Returns null where the key is absent, the value is null, or the
        /// value is not a finite number.
        /// </summary>
        public static double? NullableDouble(JsonObject jsonObject, string name)
        {
            if (jsonObject is null || name is null || !jsonObject.ContainsKey(name))
            {
                return null;
            }

            JsonNode jsonNode = jsonObject[name];
            if (jsonNode is null)
            {
                return null;
            }

            try
            {
                double result = jsonNode.GetValue<double>();
                return double.IsNaN(result) || double.IsInfinity(result) ? null : result;
            }
            catch (Exception)
            {
                //A value written by another tool that is not a number at all is treated as absent rather
                //than aborting the whole model read.
                return null;
            }
        }

        /// <summary>Writes an optional number, omitting the key entirely where there is no finite value.</summary>
        public static void SetNullableDouble(JsonObject jsonObject, string name, double? value)
        {
            if (jsonObject is null || name is null)
            {
                return;
            }

            if (value is null || double.IsNaN(value.Value) || double.IsInfinity(value.Value))
            {
                return;
            }

            jsonObject[name] = value.Value;
        }

        /// <summary>Reads a boolean, falling back to <paramref name="default"/> where the key is absent.</summary>
        public static bool Boolean(JsonObject jsonObject, string name, bool @default = false)
        {
            if (jsonObject is null || name is null || !jsonObject.ContainsKey(name))
            {
                return @default;
            }

            try
            {
                return jsonObject[name]?.GetValue<bool>() ?? @default;
            }
            catch (Exception)
            {
                return @default;
            }
        }

        /// <summary>Reads a string, returning null where the key is absent.</summary>
        public static string String(JsonObject jsonObject, string name)
        {
            if (jsonObject is null || name is null || !jsonObject.ContainsKey(name))
            {
                return null;
            }

            try
            {
                return jsonObject[name]?.GetValue<string>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Writes a string, omitting the key entirely where there is nothing to write.</summary>
        public static void SetString(JsonObject jsonObject, string name, string value)
        {
            if (jsonObject is null || name is null || value is null)
            {
                return;
            }

            jsonObject[name] = value;
        }

        /// <summary>Reads a guid, returning <see cref="Guid.Empty"/> where the key is absent or unparseable.</summary>
        public static Guid Guid(JsonObject jsonObject, string name)
        {
            string text = String(jsonObject, name);

            return System.Guid.TryParse(text, out Guid result) ? result : System.Guid.Empty;
        }
    }
}
