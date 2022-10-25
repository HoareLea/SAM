using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Classes
{
    public class GeometryObjectModel : Core.SAMModel
    {
        private GeometryObjectCollection geometryObjectCollection;

        public GeometryObjectModel(GeometryObjectModel geometryObjectModel)
        {
            if(geometryObjectModel?.geometryObjectCollection != null)
            {
                geometryObjectCollection = new GeometryObjectCollection();
                foreach(ISAMGeometryObject sAMGeometryObject in geometryObjectModel.geometryObjectCollection)
                {
                    geometryObjectCollection.Add(sAMGeometryObject);
                }
            }
        }

        public GeometryObjectModel(JObject jObject)
        {
            FromJObject(jObject);
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                result = new JObject();
            }

            if(geometryObjectCollection != null)
            {
                JArray jArray = new JArray();

                foreach (ISAMGeometryObject sAMGeometryObject in geometryObjectCollection)
                {
                    if(sAMGeometryObject == null)
                    {
                        continue;
                    }

                    jArray.Add(sAMGeometryObject.ToJObject());
                }

                result.Add("GeometryObjects", jArray);
            }
            return result;
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("GeometryObjects"))
            {
                JArray jArray = jObject.Value<JArray>("GeometryObjects");

                geometryObjectCollection = new GeometryObjectCollection();
                foreach (JObject jObject_GeometryObject in jArray)
                {
                    ISAMGeometryObject sAMGeometryObject = Core.Create.IJSAMObject<ISAMGeometryObject>(jObject_GeometryObject);
                    if(sAMGeometryObject != null)
                    {
                        geometryObjectCollection.Add(sAMGeometryObject);
                    }
                }
            }

            return true;
        }
    }
}
