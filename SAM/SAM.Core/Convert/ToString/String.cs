using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static string ToString(this IJSAMObject jSAMObject)
        {
            return ToString(jSAMObject, Formatting.Indented);
        }

        public static string ToString(this IJSAMObject jSAMObject, Formatting formatting)
        {
            if (jSAMObject == null)
                return null;

            JObject jObject = jSAMObject.ToJObject();
            if (jObject == null)
                return null;

            string json = JsonConvert.SerializeObject(jObject, new JsonSerializerSettings
            {
                Formatting = formatting
            });

            return json;
        }

        public static string ToString<T>(this IEnumerable<T> jSAMObjects) where T: IJSAMObject
        {
            return ToString(jSAMObjects, Formatting.Indented);
        }

        public static string ToString<T>(this IEnumerable<T> jSAMObjects, Formatting formatting) where T :IJSAMObject
        {
            if (jSAMObjects == null)
                return null;

            JArray jArray = new JArray();
            foreach (T jSAMObject in jSAMObjects)
            {
                if(jSAMObject == null)
                {
                    continue;
                }

                jArray.Add(jSAMObject.ToJObject());
            }


            string json = JsonConvert.SerializeObject(jArray, new JsonSerializerSettings
            {
                Formatting = formatting
            });

            return json;
        }

        public static string ToString<T>(IEnumerable<T> sAMObjects, string path) where T : IJSAMObject
        {
            if (sAMObjects == null)
                return null;

            string @string = ToString(sAMObjects.Cast<IJSAMObject>());

            File.WriteAllText(path, @string);

            return @string;
        }

        public static string ToString(Color color)
        {
            return color.Name;
        }

        public static string ToString(double value, string prefix)
        {
            if (value < 0)
                return string.Format("-{0}{1}", prefix, System.Math.Abs(value));

            return "+" + prefix + value.ToString();
        }
    }
}