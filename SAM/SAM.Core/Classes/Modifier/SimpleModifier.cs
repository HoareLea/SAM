using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class SimpleModifier : Modifier, ISimpleModifier
    {
        public ArithmeticOperator ArithmeticOperator { get; set; }

        public SimpleModifier()
            :base()
        {

        }

        public SimpleModifier(SimpleModifier simpleModifier)
            : base(simpleModifier)
        {
            if(simpleModifier != null)
            {
                ArithmeticOperator = simpleModifier.ArithmeticOperator;
            }
        }

        public SimpleModifier(JObject jObject)
            : base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return result;
            }

            if (jObject.ContainsKey("ArithmeticOperator"))
            {
                ArithmeticOperator = Query.Enum<ArithmeticOperator>(jObject.Value<string>("ArithmeticOperator"));
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return result;
            }

            result.Add("ArithmeticOperator", ArithmeticOperator.ToString());

            return result;
        }
    }
}