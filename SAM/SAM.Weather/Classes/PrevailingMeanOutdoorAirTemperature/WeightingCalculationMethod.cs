using Newtonsoft.Json.Linq;

namespace SAM.Weather
{
    public class WeightingCalculationMethod : SimpleArithmeticMeanCalculationMethod
    {
        public double Alpha { get; set; } = 0.8;

        public WeightingCalculationMethod()
            :base()
        {

        }

        public WeightingCalculationMethod(int sequentialDays, double alpha)
            :base(sequentialDays)
        {
            Alpha = alpha;
        }

        public WeightingCalculationMethod(WeightingCalculationMethod weightingCalculationMethod)
            :base(weightingCalculationMethod)
        {
            if(weightingCalculationMethod != null)
            {
                Alpha = weightingCalculationMethod.Alpha;
            }
        }

        public WeightingCalculationMethod(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Alpha"))
            {
                Alpha = jObject.Value<double>("Alpha");
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            result.Add("Alpha", Alpha);
            
            return result;
        }
    }
}