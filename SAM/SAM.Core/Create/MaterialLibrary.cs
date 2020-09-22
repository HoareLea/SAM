
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SAM.Core
{
    public static partial class Create
    {
        public static MaterialLibrary MaterialLibrary(this string path)
        {
            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path);

            JObject jObject = JToken.Parse(json) as JObject;
            if (jObject == null)
                return null;

            return new MaterialLibrary(jObject);
        }

        public static MaterialLibrary MaterialLibrary(string name, IEnumerable<IMaterial> materials)
        {
            MaterialLibrary result = new MaterialLibrary(name);

            if (materials == null)
                return result;

            foreach (IMaterial material in materials)
                result.Add(material);

            return result;
        }
    }
}