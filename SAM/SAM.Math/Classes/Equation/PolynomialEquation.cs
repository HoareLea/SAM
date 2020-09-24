using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Math
{
    public class PolynomialEquation : IJSAMObject
    {
        private double[] variables;
        
        public PolynomialEquation(JObject jObject)
        {
            FromJObject(jObject);
        }
        
        public PolynomialEquation(IEnumerable<double> variables)
        {
            if (variables == null)
                return;

            int count = variables.Count();

            this.variables = new double[count];
            for (int i = 0; i < count; i++)
                this.variables[i] = variables.ElementAt(i);
        }

        public PolynomialEquation(PolynomialEquation polynomialEquation)
        {
            if (polynomialEquation == null || polynomialEquation.variables == null)
                return;

            int count = polynomialEquation.variables.Length;

            variables = new double[count];
            for (int i = 0; i < count; i++)
                variables[i] = polynomialEquation.variables[i];
        }

        public double Calculate(double value)
        {
            double result = 0;
            for (int i = 0; i < variables.Length; i++)
                result += System.Math.Pow(value, i) * variables[i];

            return result;
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if(jObject.ContainsKey("Variables"))
            {
                JArray jArray = jObject.Value<JArray>("Variables");
                variables = new double[jArray.Count];
                for(int i=0; i < jArray.Count; i++)
                {
                    object @object = jArray[i];
                    if(@object is double)
                    {
                        variables[i] = (double)@object;
                        continue;
                    }    

                    if (!Core.Query.IsNumeric(@object))
                        return false;

                    variables[i] = System.Convert.ToDouble(@object);
                }
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (variables != null)
            {
                JArray jArray = new JArray();
                foreach (double variable in variables)
                    jArray.Add(variable);

                jObject.Add("Variables", jArray);
            }

            return jObject;
        }
    }
}
