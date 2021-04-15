using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Math
{

    /// <summary>
    /// Equation in format a(n)*x^(n) + a(n-1)*x^(n-1) + a(n-2)*x^(n-2) + [...] + a(1)*x +a(0) = 0
    /// </summary>
    public class PolynomialEquation : IJSAMObject
    {
        private double[] coefficients;
        
        public PolynomialEquation(JObject jObject)
        {
            FromJObject(jObject);
        }
        
        public PolynomialEquation(IEnumerable<double> coefficients)
        {
            if (coefficients == null)
                return;

            int count = coefficients.Count();

            this.coefficients = new double[count];
            for (int i = 0; i < count; i++)
                this.coefficients[i] = coefficients.ElementAt(i);
        }

        public PolynomialEquation(PolynomialEquation polynomialEquation)
        {
            if (polynomialEquation == null || polynomialEquation.coefficients == null)
                return;

            int count = polynomialEquation.coefficients.Length;

            coefficients = new double[count];
            for (int i = 0; i < count; i++)
                coefficients[i] = polynomialEquation.coefficients[i];
        }

        public double Evaluate(double value)
        {
            int count = coefficients.Length;

            double result = 0;
            for (int i = 0; i < count; i++)
                result += System.Math.Pow(value, count - i) * coefficients[i];

            return result;
        }

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

        public int Degree
        {
            get
            {
                if (coefficients == null)
                    return -1;

                return coefficients.Length;
            }
        }
    }
}
