using Newtonsoft.Json.Linq;

namespace SAM.Weather
{
    public class SimpleArithmeticMeanCalculationMethod : IPrevailingMeanOutdoorAirTemperatureCalculationMethod
    {
        public int SequentialDays { get; set; } = 15;

        public SimpleArithmeticMeanCalculationMethod()
        {

        }

        public SimpleArithmeticMeanCalculationMethod(int sequentialDays)
        {
            SequentialDays = sequentialDays;
        }

        public SimpleArithmeticMeanCalculationMethod(SimpleArithmeticMeanCalculationMethod simpleArithmeticMeanCalculationMethod)
        {
            if(simpleArithmeticMeanCalculationMethod != null)
            {
                SequentialDays = simpleArithmeticMeanCalculationMethod.SequentialDays;
            }
        }

        public SimpleArithmeticMeanCalculationMethod(JObject jObject)
        {
            FromJObject(jObject);
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("SequentialDays"))
            {
                SequentialDays = jObject.Value<int>("SequentialDays");
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));
            jObject.Add("SequentialDays", SequentialDays);
            return jObject;
        }
    }
}