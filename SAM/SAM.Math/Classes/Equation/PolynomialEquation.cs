// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SAM.Math
{

    /// <summary>
    /// Represents a polynomial equation of the form a(n)*x^(n) + a(n-1)*x^(n-1) + a(n-2)*x^(n-2) + [...] + a(1)*x + a(0) = 0.
    /// </summary>
    public class PolynomialEquation : IEquation
    {
        private double[] coefficients;

        public PolynomialEquation(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }


        /// <summary>
        /// Initializes a new instance of the PolynomialEquation class using coefficients.
        /// </summary>
        /// <param name="coefficients">The coefficients of the polynomial equation.</param>
        public PolynomialEquation(IEnumerable<double> coefficients)
        {
            if (coefficients == null)
                return;

            int count = coefficients.Count();

            this.coefficients = new double[count];
            for (int i = 0; i < count; i++)
                this.coefficients[i] = coefficients.ElementAt(i);
        }

        /// <summary>
        /// Initializes a new instance of the PolynomialEquation class using another PolynomialEquation object.
        /// </summary>
        /// <param name="polynomialEquation">The PolynomialEquation to copy from.</param>
        public PolynomialEquation(PolynomialEquation polynomialEquation)
        {
            if (polynomialEquation == null || polynomialEquation.coefficients == null)
                return;

            int count = polynomialEquation.coefficients.Length;

            coefficients = new double[count];
            for (int i = 0; i < count; i++)
                coefficients[i] = polynomialEquation.coefficients[i];
        }

        /// <summary>
        /// Evaluates the polynomial equation for a given x value.
        /// </summary>
        /// <param name="value">The x value to evaluate the polynomial equation for.</param>
        /// <returns>The result of the polynomial equation.</returns>
        public double Evaluate(double value)
        {
            int count = coefficients.Length;

            double result = 0;
            for (int i = 1; i < count; i++)
                result += System.Math.Pow(value, i) * coefficients[i];

            result += coefficients[0];

            return result;
        }

        /// <summary>
        /// Evaluates the polynomial equation for a given set of x values.
        /// </summary>
        /// <param name="values">The x values to evaluate the polynomial equation for.</param>
        /// <returns>The results of the polynomial equation.</returns>
        public List<double> Evaluate(IEnumerable<double> values)
        {
            if (values == null)
                return null;

            List<double> result = null;


            int count = coefficients.Length;
            if (count < 5 || values.Count() < 1000)
            {
                result = new List<double>();
                foreach (double value in values)
                {
                    result.Add(Evaluate(value));
                }
            }
            else
            {
                count = values.Count();

                result = Enumerable.Repeat(double.NaN, count).ToList();
                Parallel.For(0, count, (int i) =>
                {
                    result[i] = Evaluate(values.ElementAt(i));
                });
            }

            return result;
        }

        /// <summary>
        /// Gets the coefficients of the polynomial equation.
        /// </summary>
        public List<double> Coefficients
        {
            get
            {
                if (coefficients == null)
                    return null;

                List<double> result = new List<double>();
                foreach (double variable in coefficients)
                    result.Add(variable);

                return result;
            }
        }

        public virtual bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
                return false;

            if (jsonObject["Variables"] is JsonArray jsonArray)
            {
                coefficients = new double[jsonArray.Count];
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    //Core.Query.TryGetDouble rather than a CLR type test on GetValue<object>(): a parsed
                    //JsonValue holds a JsonElement, which no such test recognises, so reading a saved file
                    //bailed out on the first coefficient and left the whole polynomial at zero.
                    if (jsonArray[i] == null || !jsonArray[i].TryGetDouble(out double coefficient))
                        return false;

                    coefficients[i] = coefficient;
                }
            }

            return true;
        }

        public virtual JsonObject ToJsonObject()
        {
            JsonObject jsonObject = new JsonObject
            {
                ["_type"] = Core.Query.FullTypeName(this)
            };

            if (coefficients != null)
            {
                JsonArray jsonArray = new JsonArray();
                foreach (double variable in coefficients)
                    jsonArray.Add(variable);

                jsonObject["Variables"] = jsonArray;
            }

            return jsonObject;
        }

        /// <summary>
        /// Gets the degree of the polynomial equation.
        /// </summary>
        public int Degree
        {
            get
            {
                if (coefficients == null)
                    return -1;

                return coefficients.Length - 1;
            }
        }

        /// <summary>
        /// Implicitly converts a LinearEquation to a PolynomialEquation.
        /// </summary>
        /// <param name="linearEquation">The LinearEquation to convert.</param>
        public static implicit operator PolynomialEquation(LinearEquation linearEquation)
        {
            if (linearEquation == null)
                return null;

            return new PolynomialEquation(linearEquation.Coefficients);
        }
    }
}
