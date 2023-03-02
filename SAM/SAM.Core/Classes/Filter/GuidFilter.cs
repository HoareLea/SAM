using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public class GuidFilter : TextFilter
    {
        public GuidFilter(JObject jObject)
            :base(jObject)
        {

        }

        public GuidFilter(GuidFilter guidFilter)
            :base(guidFilter)
        {

        }

        public GuidFilter(TextComparisonType textComparisonType, string value)
            :base(textComparisonType, value)
        {

        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            ISAMObject sAMObject = jSAMObject as ISAMObject;
            if(sAMObject == null)
            {
                return false;
            }

            string guidString = sAMObject.Guid.ToString();

            bool result = Query.Compare(guidString, Value, TextComparisonType);
            if(Inverted)
            {
                result = !result;
            }

            return result;
        }
    }
}
