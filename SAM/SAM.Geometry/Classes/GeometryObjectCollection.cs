using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public class GeometryObjectCollection : IEnumerable<ISAMGeometryObject>, ISAMGeometryObject
    {
        private List<ISAMGeometryObject> sAMGeometryObjects;

        public GeometryObjectCollection()
        {
            sAMGeometryObjects = new List<ISAMGeometryObject>();
        }

        public GeometryObjectCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public GeometryObjectCollection(GeometryObjectCollection geometryObjectCollection)
        {
            if (geometryObjectCollection != null)
            {
                List<ISAMGeometryObject> sAMGeometryObjects_Temp = geometryObjectCollection.sAMGeometryObjects;
                if(sAMGeometryObjects_Temp != null)
                {
                    sAMGeometryObjects = new List<ISAMGeometryObject>();
                    foreach (ISAMGeometryObject sAMGeometryObject in sAMGeometryObjects_Temp)
                    {
                        ISAMGeometryObject sAMGeometryObject_Temp = Core.Query.Clone(sAMGeometryObject);
                        if(sAMGeometryObject_Temp != null)
                        {
                            sAMGeometryObjects.Add(sAMGeometryObject_Temp);
                        }
                    }
                }
            }
        }

        public GeometryObjectCollection(IEnumerable<ISAMGeometryObject> sAMGeometryObjects)
        {
            if (sAMGeometryObjects != null)
            {
                sAMGeometryObjects = new List<ISAMGeometryObject>(sAMGeometryObjects);
            }
        }

        public GeometryObjectCollection(ISAMGeometryObject sAMGeometryObject)
        {
            sAMGeometryObjects = new List<ISAMGeometryObject>() { sAMGeometryObject };
        }

        public void Add(ISAMGeometryObject sAMGeometryObject)
        {
            sAMGeometryObjects.Add(sAMGeometryObject);
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("GeometryObjects"))
            {
                sAMGeometryObjects = new List<ISAMGeometryObject>();

                JArray jArray = jObject.Value<JArray>("GeometryObjects");
                foreach(JObject jObject_GeometryObject in jArray)
                {
                    ISAMGeometryObject sAMGeometryObject = Core.Query.IJSAMObject(jObject_GeometryObject) as ISAMGeometryObject;
                    if(sAMGeometryObject != null)
                    {
                        sAMGeometryObjects.Add(sAMGeometryObject);
                    }
                }
            }

            return true;
        }

        public IEnumerator<ISAMGeometryObject> GetEnumerator()
        {
            return sAMGeometryObjects?.GetEnumerator();
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(sAMGeometryObjects != null)
            {
                JArray jArray = new JArray();
                foreach(ISAMGeometryObject sAMGeometryObject in sAMGeometryObjects)
                {
                    if(sAMGeometryObject == null)
                    {
                        continue;
                    }

                    jArray.Add(sAMGeometryObject.ToJObject());
                }

                jObject.Add("GeometryObjects", jArray);
            }

            return jObject;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return sAMGeometryObjects?.GetEnumerator();
        }
    }
}
