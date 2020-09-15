using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class ConstructionLayer : IJSAMObject
    {
        private double thickness;
        private string name;

        public ConstructionLayer(string name, double thickness)
        {
            this.thickness = thickness;
            this.name = name;
        }

        public ConstructionLayer(ConstructionLayer constructionLayer)
        {
            thickness = constructionLayer.thickness;
            name = constructionLayer.name;
        }

        public ConstructionLayer(JObject jObject)
        {
            FromJObject(jObject);
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Thickness"))
                thickness = jObject.Value<double>("Thickness");

            if (jObject.ContainsKey("Name"))
                name = jObject.Value<string>("Name");

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (name != null)
                jObject.Add("Name", name);

            if (!double.IsNaN(thickness))
                jObject.Add("Thickness", thickness);

            return jObject;
        }
    }
}