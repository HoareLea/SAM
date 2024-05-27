using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class SimpleModifier : Modifier
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

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("ArithmeticOperator"))
            {
                ArithmeticOperator = Query.Enum<ArithmeticOperator>(jObject.Value<string>("ArithmeticOperator"));
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            jObject.Add("ArithmeticOperator", ArithmeticOperator.ToString());

            return jObject;
        }
    }
}