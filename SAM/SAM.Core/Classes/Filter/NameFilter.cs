using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public class NameFilter : TextFilter
    {
        public NameFilter(JObject jObject)
            : base(jObject)
        {

        }

        public NameFilter(NameFilter nameFilter)
            :base(nameFilter)
        {

        }

        public NameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {

        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            ISAMObject sAMObject = jSAMObject as ISAMObject;
            if (sAMObject == null)
            {
                return false;
            }

            string name = sAMObject.Name;

            bool result = Query.Compare(name, Value, TextComparisonType);
            if (Inverted)
            {
                result = !result;
            }

            return result;
        }
    }
}
