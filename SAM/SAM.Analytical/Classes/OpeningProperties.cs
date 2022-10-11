using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    /// <summary>
    /// Opening Properties
    /// </summary>
    public class OpeningProperties : IJSAMObject
    {
        public double OpenableArea { get; set; }

        public OpeningProperties()
        {

        }

        public OpeningProperties(JObject jObject)
        {
            FromJObject(jObject);
        }

        public OpeningProperties(OpeningProperties openingProperties)
        {
            if(openingProperties != null)
            {
                OpenableArea = openingProperties.OpenableArea;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("OpenableArea"))
            {
                OpenableArea = jObject.Value<double>("OpenableArea");
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(!double.IsNaN(OpenableArea))
            {
                jObject.Add("OpenableArea", OpenableArea);
            }

            return jObject;
        }
    }
}