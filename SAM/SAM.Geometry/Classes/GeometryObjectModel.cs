using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Geometry
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

        public GeometryObjectModel()
        {

        }

        public bool Add(ISAMGeometryObject sAMGeometryObject)
        {
            if(sAMGeometryObject == null)
            {
                return false;
            }

            geometryObjectCollection.Add(sAMGeometryObject);
            return true;
        }

        public List<T> GetSAMGeometryObjects<T>(Func<T, bool> func = null) where T : ISAMGeometryObject
        {
            if(geometryObjectCollection == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (ISAMGeometryObject sAMGeometryObject in geometryObjectCollection)
            {
                if(!(sAMGeometryObject is T))
                {
                    continue;
                }

                T t = (T)sAMGeometryObject;

                if (func != null)
                {
                    if(!func.Invoke(t))
                    {
                        continue;
                    }
                }

                result.Add(t);
            }

            return result;
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
