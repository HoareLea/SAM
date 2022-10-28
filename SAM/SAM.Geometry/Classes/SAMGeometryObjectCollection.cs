using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public class SAMGeometryObjectCollection : IEnumerable<ISAMGeometryObject>, ISAMGeometryObject
    {
        private List<ISAMGeometryObject> sAMGeometryObjects;

        public SAMGeometryObjectCollection()
        {
            sAMGeometryObjects = new List<ISAMGeometryObject>();
        }

        public SAMGeometryObjectCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public SAMGeometryObjectCollection(SAMGeometryObjectCollection sAMGeometryObjectCollection)
        {
            if (sAMGeometryObjectCollection != null)
            {
                List<ISAMGeometryObject> sAMGeometryObjects_Temp = sAMGeometryObjectCollection.sAMGeometryObjects;
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

        public SAMGeometryObjectCollection(IEnumerable<ISAMGeometryObject> sAMGeometryObjects)
        {
            if (sAMGeometryObjects != null)
            {
                this.sAMGeometryObjects = new List<ISAMGeometryObject>();
                foreach(ISAMGeometryObject sAMGeometryObject in sAMGeometryObjects)
                {
                    ISAMGeometryObject sAMGeometryObject_Temp = Core.Query.Clone(sAMGeometryObject);
                    if(sAMGeometryObject_Temp != null)
                    {
                        this.sAMGeometryObjects.Add(sAMGeometryObject_Temp);
                    }
                }
            }
        }

        public SAMGeometryObjectCollection(ISAMGeometryObject sAMGeometryObject)
        {
            ISAMGeometryObject sAMGeometryObject_Temp = Core.Query.Clone(sAMGeometryObject);
            if(sAMGeometryObject_Temp != null)
            {
                sAMGeometryObjects = new List<ISAMGeometryObject>() { sAMGeometryObject_Temp };
            }
        }

        public void Add(ISAMGeometryObject sAMGeometryObject)
        {
            ISAMGeometryObject sAMGeometryObject_Temp = Core.Query.Clone(sAMGeometryObject);
            if (sAMGeometryObject_Temp == null)
            {
                return;
            }

            if(sAMGeometryObjects == null)
            {
                sAMGeometryObjects = new List<ISAMGeometryObject>();
            }

            sAMGeometryObjects.Add(sAMGeometryObject_Temp);
        }

        public bool FromJObject(JObject jObject)
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

        public JObject ToJObject()
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
