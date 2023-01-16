using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static TextMap TextMap(string name)
        {
            return new TextMap(name);
        }

        public static TextMap TextMap(TextMap textMap)
        {
            if(textMap == null)
            {
                return null;
            }

            return new TextMap(textMap);
        }

        public static TextMap TextMap(string name, TextMap textMap)
        {
            if (textMap == null)
            {
                return null;
            }

            return new TextMap(name, textMap);
        }

        public static TextMap TextMap(JObject jObject)
        {
            if (jObject == null)
            {
                return null;
            }

            return new TextMap(jObject);
        }
    }
}