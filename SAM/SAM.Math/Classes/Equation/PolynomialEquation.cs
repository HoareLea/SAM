using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Math
{

    /// <summary>
    /// Represents a polynomial equation of the form a(n)*x^(n) + a(n-1)*x^(n-1) + a(n-2)*x^(n-2) + [...] + a(1)*x + a(0) = 0.
    /// </summary>
    public class PolynomialEquation : IEquation
    {
        private double[] coefficients;

        /// <summary>
        /// Initializes a new instance of the PolynomialEquation class using a JObject.
        /// </summary>
        /// <param name="jObject">The JObject containing the polynomial equation data.</param>
        public PolynomialEquation(JObject jObject)
        {
            FromJObject(jObject);
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
            if(count < 5 || values.Count() < 1000)
            {
                result = new List<double>();
                foreach(double value in values)
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

        /// <summary>
        /// Updates the PolynomialEquation object using a JObject.
        /// </summary>
        /// <param name="jObject">The JObject containing the polynomial equation data.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if(jObject.ContainsKey("Variables"))
            {
                JArray jArray = jObject.Value<JArray>("Variables");
                coefficients = new double[jArray.Count];
                for(int i=0; i < jArray.Count; i++)
                {
                    object @object = jArray[i];
                    if(@object is double)
                    {
                        coefficients[i] = (double)@object;
                        continue;
                    }    

                    if (!Core.Query.IsNumeric(@object))
                        return false;

                    coefficients[i] = System.Convert.ToDouble(@object);
                }
            }

            return true;
        }

        /// <summary>
        /// Converts the PolynomialEquation object to a JObject.
        /// </summary>
        /// <returns>The JObject representation of the PolynomialEquation object.</returns>
        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (coefficients != null)
            {
                JArray jArray = new JArray();
                foreach (double variable in coefficients)
                    jArray.Add(variable);

                jObject.Add("Variables", jArray);
            }

            return jObject;
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
