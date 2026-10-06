// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Math;
using SAM.Tests.Helpers;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Round-trip cover for the numeric readers in SAM.Math and SAM.Core.Query.Array.
    /// <para>
    /// These go through a JSON <b>string</b> rather than handing a freshly built JsonObject straight
    /// back to FromJsonObject, and that is the whole point. A JsonValue built in-process holds a boxed
    /// CLR double; a JsonValue that came out of JsonNode.Parse - which is every path that reads a saved
    /// SAM file - holds a JsonElement instead. A reader that inspects the CLR type of
    /// GetValue&lt;object&gt;() sees a numeric type in the first case and JsonElement in the second, so
    /// an in-process-only test passes while every saved file deserialises empty.
    /// </para>
    /// </summary>
    public class InterpolationRoundTripTests
    {
        [Fact]
        public void RoundTrip_LinearInterpolation_PreservesValues()
        {
            LinearInterpolation expected = new LinearInterpolation();
            expected.Add(0.0, 0.0);
            expected.Add(10.0, 100.0);

            LinearInterpolation result = RoundTrip.Once(expected);

            Assert.Equal(2, result.Count);
            Assert.Equal(0.0, result.MinX);
            Assert.Equal(10.0, result.MaxX);
            Assert.Equal(50.0, result.CalculateY(5.0));
        }

        [Fact]
        public void RoundTrip_LinearInterpolation_PreservesFractionalValues()
        {
            LinearInterpolation expected = new LinearInterpolation(0.5, 1.25, 2.5, 5.25);

            LinearInterpolation result = RoundTrip.Once(expected);

            Assert.Equal(2, result.Count);
            Assert.Equal(3.25, result.CalculateY(1.5), 10);
        }

        [Fact]
        public void RoundTrip_BilinearInterpolation_PreservesValues()
        {
            // [y, x] layout: first row is the x axis, first column is the y axis.
            BilinearInterpolation expected = new BilinearInterpolation(new double[,]
            {
                {  0.0,  1.0,  2.0 },
                { 10.0,  3.0,  5.0 },
                { 20.0,  7.0, 11.0 }
            });

            BilinearInterpolation result = RoundTrip.Once(expected);

            // Exact at a node, then the centre of the cell - the mean of the four corners.
            Assert.Equal(3.0, result.Calculate(1.0, 10.0));
            Assert.Equal(11.0, result.Calculate(2.0, 20.0));
            Assert.Equal(6.5, result.Calculate(1.5, 15.0), 10);
        }

        [Fact]
        public void RoundTrip_PolynomialEquation_PreservesCoefficients()
        {
            PolynomialEquation expected = new PolynomialEquation(new double[] { 1.5, 2.0, 3.0 });

            PolynomialEquation result = RoundTrip.Once(expected);

            Assert.Equal(new double[] { 1.5, 2.0, 3.0 }, result.Coefficients);

            // 1.5 + 2*2 + 3*4
            Assert.Equal(17.5, result.Evaluate(2.0), 10);
        }

        [Fact]
        public void Array_ParsedJsonArray_ReadsNumbers()
        {
            // Awkward on purpose: an integer, a plain fraction, a value with digits either side of a
            // thousands boundary, an exponent, and a negative. Anything that reads the number back by
            // way of its text has to survive all of them exactly.
            JsonArray jsonArray = (JsonArray)JsonNode.Parse("[[1, 2.5, 1234567.25], [3, 1e-05, -0.1]]")!;

            double[,] result = Core.Query.Array<double>(jsonArray);

            Assert.NotNull(result);
            Assert.Equal(1.0, result[0, 0]);
            Assert.Equal(2.5, result[0, 1]);
            Assert.Equal(1234567.25, result[0, 2]);
            Assert.Equal(3.0, result[1, 0]);
            Assert.Equal(1e-05, result[1, 1]);
            Assert.Equal(-0.1, result[1, 2]);
        }

        [Fact]
        public void Array_ParsedJsonArray_ReadsIntegralDecimalsWithoutCultureGuessing()
        {
            // A JSON number token is invariant by definition, so a hand-written or hand-edited "-1.000"
            // is minus one. Read it by way of its text with a parser that guesses whether ',' or '.' is
            // the decimal separator and "-1.000" also reads as "-1,000" - minus one thousand - which
            // under any culture that groups with ',' is the value that wins.
            JsonArray jsonArray = (JsonArray)JsonNode.Parse("[[-1.000, 1.000], [-2.500, -1234.000]]")!;

            double[,] result = Core.Query.Array<double>(jsonArray);

            Assert.NotNull(result);
            Assert.Equal(-1.0, result[0, 0]);
            Assert.Equal(1.0, result[0, 1]);
            Assert.Equal(-2.5, result[1, 0]);
            Assert.Equal(-1234.0, result[1, 1]);
        }

        [Fact]
        public void FromJson_BilinearInterpolation_ReadsIntegralDecimals()
        {
            // The same thing where it actually bites: a matrix of temperatures a human typed with
            // trailing zeros. Whole-numbered and negative is the ordinary case here, not an edge case.
            string json = @"{
                ""_type"": ""SAM.Math.BilinearInterpolation"",
                ""XArray"": [1.000, 2.000],
                ""YArray"": [10.000, 20.000],
                ""Values"": [[-1.000, -3.000], [-5.000, -7.000]]
            }";

            BilinearInterpolation result = RoundTrip.FromJson<BilinearInterpolation>(json);

            Assert.Equal(-1.0, result.Calculate(1.0, 10.0));
            Assert.Equal(-7.0, result.Calculate(2.0, 20.0));
            Assert.Equal(-4.0, result.Calculate(1.5, 15.0), 10);
        }
    }
}
