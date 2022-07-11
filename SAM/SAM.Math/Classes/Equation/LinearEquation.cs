using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Math
{

    /// <summary>
    /// Equation in format y = Ax + B
    /// </summary>
    public class LinearEquation : IEquation
    {
        private double a;
        private double b;
        
        public LinearEquation(JObject jObject)
        {
            FromJObject(jObject);
        }
        
        public LinearEquation(double a, double b)
        {
            this.a = a;
            this.b = b;
        }

        public LinearEquation(LinearEquation linearEquation)
        {
            if(linearEquation != null)
            {
                a = linearEquation.a;
                b = linearEquation.b;
            }
        }

        public double Evaluate(double value)
        {
            if(double.IsNaN(a) || double.IsNaN(b) || double.IsNaN(value))
            {
                return double.NaN;
            }

            return b + a * value;
        }

        public List<double> Evaluate(IEnumerable<double> values)
        {
            if (values == null)
                return null;

            List<double> result = new List<double>();
            foreach(double value in values)
            {
                result.Add(Evaluate(value));
            }

            return result;
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if(jObject.ContainsKey("A"))
            {
                a = jObject.Value<double>("A");
            }

            if (jObject.ContainsKey("B"))
            {
                b = jObject.Value<double>("B");
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(!double.IsNaN(a))
            {
                jObject.Add("A", a);
            }

            if (!double.IsNaN(b))
            {
                jObject.Add("B", b);
            }

            return jObject;
        }

        public List<double> Coefficients
        {
            get
            {
                return new List<double>() { b, a};
            }
        }
    }
}
