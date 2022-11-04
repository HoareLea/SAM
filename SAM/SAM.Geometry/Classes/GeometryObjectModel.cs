using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public class GeometryObjectModel : Core.SAMModel
    {
        private SAMGeometryObjectCollection sAMGeometryObjectCollection;

        public GeometryObjectModel(GeometryObjectModel geometryObjectModel)
            :base(geometryObjectModel)
        {
            if(geometryObjectModel?.sAMGeometryObjectCollection != null)
            {
                sAMGeometryObjectCollection = new SAMGeometryObjectCollection();
                foreach(ISAMGeometryObject sAMGeometryObject in geometryObjectModel.sAMGeometryObjectCollection)
                {
                    sAMGeometryObjectCollection.Add(sAMGeometryObject);
                }
            }
        }

        public GeometryObjectModel(JObject jObject)
            :base(jObject)
        {

        }

        public GeometryObjectModel()
            :base()
        {

        }

        public bool Add(ISAMGeometryObject sAMGeometryObject)
        {
            if(sAMGeometryObject == null)
            {
                return false;
            }

            if(sAMGeometryObjectCollection == null)
            {
                sAMGeometryObjectCollection = new SAMGeometryObjectCollection();
            }

            sAMGeometryObjectCollection.Add(sAMGeometryObject);
            return true;
        }

        public List<T> GetSAMGeometryObjects<T>(Func<T, bool> func = null) where T : ISAMGeometryObject
        {
            if(sAMGeometryObjectCollection == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (ISAMGeometryObject sAMGeometryObject in sAMGeometryObjectCollection)
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

            if(sAMGeometryObjectCollection != null)
            {
                JArray jArray = new JArray();

                foreach (ISAMGeometryObject sAMGeometryObject in sAMGeometryObjectCollection)
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

                sAMGeometryObjectCollection = new SAMGeometryObjectCollection();
                foreach (JObject jObject_GeometryObject in jArray)
                {
                    ISAMGeometryObject sAMGeometryObject = Core.Create.IJSAMObject<ISAMGeometryObject>(jObject_GeometryObject);
                    if(sAMGeometryObject != null)
                    {
                        sAMGeometryObjectCollection.Add(sAMGeometryObject);
                    }
                }
            }

            return true;
        }
    }
}
