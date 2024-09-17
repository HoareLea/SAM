using Newtonsoft.Json.Linq;
using SAM.Math;

namespace SAM.Core
{
    public class PolynomialModifier : IndexedSimpleModifier
    {
        public PolynomialEquation PolynomialEquation { get; set; }

        public PolynomialModifier(ArithmeticOperator arithmeticOperator, PolynomialEquation polynomialEquation)
        {
            ArithmeticOperator = arithmeticOperator;
            this.PolynomialEquation = polynomialEquation == null ? null : new PolynomialEquation(polynomialEquation);
        }

        public PolynomialModifier(PolynomialModifier polynomialModifier)
            : base(polynomialModifier)
        {
            if(polynomialModifier != null)
            {
                PolynomialEquation = polynomialModifier?.PolynomialEquation == null ? null : new PolynomialEquation(polynomialModifier.PolynomialEquation);
            }
        }

        public PolynomialModifier(JObject jObject)
            :base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return result;
            }

            if(jObject.ContainsKey("PolynomialEquation"))
            {
                PolynomialEquation = Query.IJSAMObject<PolynomialEquation>(jObject.Value<JObject>("PolynomialEquation"));
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            if(PolynomialEquation != null)
            {
                result.Add("PolynomialEquation", PolynomialEquation.ToJObject());
            }

            return result;
        }

        public override bool ContainsIndex(int index)
        {
            return PolynomialEquation != null;
        }

        public override double GetCalculatedValue(int index, double value)
        {
            if (PolynomialEquation == null)
            {
                return value;
            }

            return Query.Calculate(ArithmeticOperator, value, PolynomialEquation.Evaluate(index));
        }
    }
}