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
            : base(nameFilter)
        {
        }

        public NameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {
        }

        public override bool TryGetText(IJSAMObject jSAMObject, out string text)
        {
            text = null;
            ISAMObject sAMObject = jSAMObject as ISAMObject;
            if (sAMObject == null)
            {
                return false;
            }

            text = sAMObject.Name;
            return true;
        }
    }
}