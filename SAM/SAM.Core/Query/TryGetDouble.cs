// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Text.Json.Nodes;

namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Reads a JSON number out of <paramref name="jsonNode"/> as a double.
        /// </summary>
        /// <remarks>
        /// The obvious spelling - <c>jsonNode.GetValue&lt;object&gt;()</c> followed by a CLR type test such
        /// as <see cref="IsNumeric(object)"/> - is wrong, and wrong only on the paths that matter. A
        /// <see cref="JsonValue"/> built in-process wraps a boxed CLR number, so the type test sees
        /// <see cref="double"/> and passes; a <see cref="JsonValue"/> that came out of
        /// <see cref="JsonNode.Parse(string, JsonNodeOptions?, System.Text.Json.JsonDocumentOptions)"/> -
        /// which is every path that reads a saved SAM file - wraps a
        /// <see cref="System.Text.Json.JsonElement"/> instead, and the same test fails on every value. The
        /// symptom is not an exception but an object that deserialises silently empty, so it survives any
        /// test that keeps the JsonObject in memory rather than going through a string.
        /// <para>
        /// <c>TryGetValue</c> handles both, but only for the numeric shape actually stored, so the shapes
        /// System.Text.Json can hand back are tried in turn. A JSON string is deliberately not parsed: the
        /// CLR type test it replaces rejected strings too, and quietly accepting them here would widen what
        /// counts as a number in every caller at once.
        /// </para>
        /// </remarks>
        /// <param name="jsonNode">Node to read. Null, a non-value node, and a non-numeric value all fail.</param>
        /// <param name="result">The value read, or <see cref="double.NaN"/> when this returns false.</param>
        /// <returns>True if a number was read.</returns>
        public static bool TryGetDouble(this JsonNode jsonNode, out double result)
        {
            result = double.NaN;

            if (!(jsonNode is JsonValue jsonValue))
            {
                return false;
            }

            if (jsonValue.TryGetValue(out double value_Double))
            {
                result = value_Double;
                return true;
            }

            if (jsonValue.TryGetValue(out long value_Long))
            {
                result = value_Long;
                return true;
            }

            if (jsonValue.TryGetValue(out decimal value_Decimal))
            {
                result = (double)value_Decimal;
                return true;
            }

            //Anything System.Text.Json stores as some other boxed numeric CLR type - an int written by a
            //caller that added an int to a JsonArray, for instance - still reaches the original test.
            object @object = jsonValue.GetValue<object>();
            if (IsNumeric(@object))
            {
                result = System.Convert.ToDouble(@object);
                return true;
            }

            return false;
        }
    }
}
